/**
 * Test data fixtures for Sterling Auctions E2E tests
 */
export const testData = {
  users: {
    validUser: {
      email: 'test@example.com',
      password: 'TestPassword123!',
      firstName: 'Test',
      lastName: 'User'
    },
    adminUser: {
      email: 'admin@example.com',
      password: 'AdminPassword123!',
      firstName: 'Admin',
      lastName: 'User'
    }
  },

  auctions: {
    validAuction: {
      title: 'Test Auction Item',
      description: 'This is a test auction item for E2E testing',
      startingBid: 100,
      category: 'Electronics',
      duration: 7 // days
    },
    endingSoonAuction: {
      title: 'Auction Ending Soon',
      description: 'This auction is ending soon for testing',
      startingBid: 50,
      category: 'Books',
      duration: 1 // day
    }
  },

  bids: {
    validBid: {
      amount: 150,
      isAutoBid: false
    },
    autoBid: {
      amount: 200,
      maxAmount: 300,
      isAutoBid: true
    }
  },

  categories: [
    'Electronics',
    'Books',
    'Art',
    'Jewelry',
    'Collectibles',
    'Sports',
    'Home & Garden',
    'Automotive'
  ],

  apiEndpoints: {
    auth: {
      login: '/api/auth/login',
      register: '/api/auth/register',
      profile: '/api/auth/profile'
    },
    auctions: {
      list: '/api/auctions',
      create: '/api/auctions',
      details: '/api/auctions/{id}',
      bid: '/api/auctions/{id}/bid'
    },
    payments: {
      create: '/api/payment/create',
      confirm: '/api/payment/confirm',
      history: '/api/payment/history'
    }
  },

  selectors: {
    // Navigation
    nav: {
      home: '[data-testid="nav-home"]',
      auctions: '[data-testid="nav-auctions"]',
      profile: '[data-testid="nav-profile"]',
      login: '[data-testid="nav-login"]',
      logout: '[data-testid="nav-logout"]'
    },

    // Forms
    forms: {
      login: {
        email: '[data-testid="login-email"]',
        password: '[data-testid="login-password"]',
        submit: '[data-testid="login-submit"]'
      },
      register: {
        firstName: '[data-testid="register-firstName"]',
        lastName: '[data-testid="register-lastName"]',
        email: '[data-testid="register-email"]',
        password: '[data-testid="register-password"]',
        confirmPassword: '[data-testid="register-confirmPassword"]',
        submit: '[data-testid="register-submit"]'
      },
      auction: {
        title: '[data-testid="auction-title"]',
        description: '[data-testid="auction-description"]',
        startingBid: '[data-testid="auction-startingBid"]',
        category: '[data-testid="auction-category"]',
        duration: '[data-testid="auction-duration"]',
        submit: '[data-testid="auction-submit"]'
      },
      bid: {
        amount: '[data-testid="bid-amount"]',
        submit: '[data-testid="bid-submit"]',
        autoBid: '[data-testid="bid-autoBid"]',
        maxAmount: '[data-testid="bid-maxAmount"]'
      }
    },

    // Components
    components: {
      notificationCenter: '[data-testid="notification-center"]',
      connectionStatus: '[data-testid="connection-status"]',
      auctionCard: '[data-testid="auction-card"]',
      bidHistory: '[data-testid="bid-history"]',
      countdownTimer: '[data-testid="countdown-timer"]',
      toastNotification: '[data-testid="toast-notification"]',
      modal: '[data-testid="modal"]',
      modalClose: '[data-testid="modal-close"]'
    },

    // Pages
    pages: {
      home: '[data-testid="home-page"]',
      auctions: '[data-testid="auctions-page"]',
      auctionDetail: '[data-testid="auction-detail-page"]',
      profile: '[data-testid="profile-page"]',
      login: '[data-testid="login-page"]',
      register: '[data-testid="register-page"]'
    }
  },

  messages: {
    success: {
      login: 'Successfully logged in',
      register: 'Account created successfully',
      bidPlaced: 'Bid placed successfully',
      auctionCreated: 'Auction created successfully'
    },
    error: {
      login: 'Invalid email or password',
      register: 'Registration failed',
      bidTooLow: 'Bid amount is too low',
      auctionEnded: 'This auction has ended'
    },
    validation: {
      required: 'This field is required',
      email: 'Please enter a valid email address',
      password: 'Password must be at least 8 characters',
      bidAmount: 'Bid amount must be greater than current bid'
    }
  },

  timeouts: {
    short: 5000,
    medium: 10000,
    long: 30000,
    veryLong: 60000
  }
};
