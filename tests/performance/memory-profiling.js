const { performance } = require('perf_hooks');
const fs = require('fs');

// Memory profiling configuration
const config = {
  duration: 60000, // 1 minute
  interval: 1000, // 1 second
  url: 'http://localhost:3000'
};

// Memory usage tracking
const memoryUsage = [];
const startTime = performance.now();

console.log('Starting memory profiling...');
console.log(`Target URL: ${config.url}`);
console.log(`Duration: ${config.duration / 1000} seconds`);
console.log(`Interval: ${config.interval / 1000} seconds`);

// Function to get memory usage
function getMemoryUsage() {
  const usage = process.memoryUsage();
  return {
    timestamp: Date.now(),
    rss: Math.round(usage.rss / 1024 / 1024), // MB
    heapTotal: Math.round(usage.heapTotal / 1024 / 1024), // MB
    heapUsed: Math.round(usage.heapUsed / 1024 / 1024), // MB
    external: Math.round(usage.external / 1024 / 1024), // MB
    arrayBuffers: Math.round(usage.arrayBuffers / 1024 / 1024), // MB
  };
}

// Function to make HTTP request
async function makeRequest() {
  try {
    const response = await fetch(config.url);
    return {
      status: response.status,
      ok: response.ok,
      timestamp: Date.now()
    };
  } catch (error) {
    return {
      error: error.message,
      timestamp: Date.now()
    };
  }
}

// Main profiling loop
async function profileMemory() {
  const interval = setInterval(async () => {
    const memory = getMemoryUsage();
    const request = await makeRequest();
    
    memoryUsage.push({
      memory,
      request,
      elapsed: performance.now() - startTime
    });

    console.log(`Memory: RSS=${memory.rss}MB, Heap=${memory.heapUsed}/${memory.heapTotal}MB, External=${memory.external}MB`);
  }, config.interval);

  // Stop profiling after duration
  setTimeout(() => {
    clearInterval(interval);
    
    // Calculate statistics
    const stats = calculateStats();
    
    // Save results
    const results = {
      config,
      stats,
      samples: memoryUsage
    };
    
    fs.writeFileSync('memory-profiling-results.json', JSON.stringify(results, null, 2));
    
    console.log('\nMemory profiling completed!');
    console.log('Results saved to memory-profiling-results.json');
    console.log('\nStatistics:');
    console.log(`Peak RSS: ${stats.peakRss}MB`);
    console.log(`Peak Heap: ${stats.peakHeap}MB`);
    console.log(`Average RSS: ${stats.avgRss}MB`);
    console.log(`Average Heap: ${stats.avgHeap}MB`);
    console.log(`Memory Growth: ${stats.memoryGrowth}MB`);
    
    process.exit(0);
  }, config.duration);
}

// Calculate memory statistics
function calculateStats() {
  const rssValues = memoryUsage.map(sample => sample.memory.rss);
  const heapValues = memoryUsage.map(sample => sample.memory.heapUsed);
  
  return {
    peakRss: Math.max(...rssValues),
    peakHeap: Math.max(...heapValues),
    avgRss: Math.round(rssValues.reduce((a, b) => a + b, 0) / rssValues.length),
    avgHeap: Math.round(heapValues.reduce((a, b) => a + b, 0) / heapValues.length),
    memoryGrowth: rssValues[rssValues.length - 1] - rssValues[0],
    sampleCount: memoryUsage.length
  };
}

// Start profiling
profileMemory().catch(console.error);
