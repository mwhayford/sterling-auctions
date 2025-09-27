// User types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatar?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export enum UserRole {
  SUPER_ADMIN = 'super_admin',
  ADMIN = 'admin',
  MODERATOR = 'moderator',
  MEMBER = 'member',
  GUEST = 'guest',
}

export enum Permission {
  USER_MANAGE = 'user:manage',
  AUCTION_MANAGE = 'auction:manage',
  PAYMENT_VIEW = 'payment:view',
  ANALYTICS_VIEW = 'analytics:view',
  SYSTEM_CONFIG = 'system:config',
}

// Authentication types
export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken: string;
  expiresAt: string;
}

// Auction types
export interface Auction {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingBid: number;
  currentBid: number;
  bidCount: number;
  status: AuctionStatus;
  category: string;
  images: string[];
  seller: User;
  winner?: User;
  createdAt: string;
  updatedAt: string;
}

export enum AuctionStatus {
  DRAFT = 'draft',
  SCHEDULED = 'scheduled',
  ACTIVE = 'active',
  ENDED = 'ended',
  CANCELLED = 'cancelled',
}

export interface Bid {
  id: string;
  auctionId: string;
  userId: string;
  amount: number;
  timestamp: string;
  user: User;
}

export interface CreateAuctionData {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  startingBid: number;
  category: string;
  images: File[];
}

// Payment types
export interface PaymentMethod {
  id: string;
  type: PaymentMethodType;
  last4?: string;
  brand?: string;
  expiryMonth?: number;
  expiryYear?: number;
  isDefault: boolean;
  createdAt: string;
}

export enum PaymentMethodType {
  CARD = 'card',
  PAYPAL = 'paypal',
  BANK_TRANSFER = 'bank_transfer',
}

export interface Payment {
  id: string;
  userId: string;
  auctionId: string;
  amount: number;
  status: PaymentStatus;
  paymentMethod: PaymentMethod;
  transactionId: string;
  createdAt: string;
  updatedAt: string;
}

export enum PaymentStatus {
  PENDING = 'pending',
  PROCESSING = 'processing',
  COMPLETED = 'completed',
  FAILED = 'failed',
  REFUNDED = 'refunded',
}

// Analytics types
export interface AnalyticsData {
  totalUsers: number;
  totalAuctions: number;
  totalRevenue: number;
  activeAuctions: number;
  userGrowth: ChartData[];
  revenueGrowth: ChartData[];
  auctionsByCategory: CategoryData[];
  topSellingItems: Auction[];
}

export interface ChartData {
  date: string;
  value: number;
  label?: string;
}

export interface CategoryData {
  category: string;
  count: number;
  revenue: number;
}

// API types
export interface ApiResponse<T = any> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T = any> {
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
}

export interface ApiError {
  message: string;
  code: string;
  details?: Record<string, string[]>;
}

// Form types
export interface FormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'file';
  placeholder?: string;
  required?: boolean;
  validation?: Record<string, any>;
  options?: { value: string; label: string }[];
}

// Feature flags
export interface FeatureFlag {
  name: string;
  enabled: boolean;
  rolloutPercentage: number;
  targetUsers: string[];
  conditions: Record<string, any>;
}

// Notification types
export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  data?: Record<string, any>;
}

export enum NotificationType {
  BID_PLACED = 'bid_placed',
  BID_OUTBID = 'bid_outbid',
  AUCTION_WON = 'auction_won',
  AUCTION_ENDED = 'auction_ended',
  PAYMENT_RECEIVED = 'payment_received',
  PAYMENT_FAILED = 'payment_failed',
  SYSTEM_ANNOUNCEMENT = 'system_announcement',
}

// Component types
export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  render?: (value: any, row: any) => React.ReactNode;
  className?: string;
}

export interface TableProps {
  columns: TableColumn[];
  data: any[];
  loading?: boolean;
  sortKey?: string;
  sortDirection?: 'asc' | 'desc';
  onSort?: (key: string, direction: 'asc' | 'desc') => void;
  onRowClick?: (row: any) => void;
  emptyMessage?: string;
}

// Theme types
export interface Theme {
  colors: {
    primary: string;
    secondary: string;
    success: string;
    warning: string;
    error: string;
    background: string;
    surface: string;
    text: string;
  };
  spacing: Record<string, string>;
  typography: {
    fontFamily: string;
    fontSize: Record<string, string>;
    fontWeight: Record<string, number>;
  };
}

// Utility types
export type Optional<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>;
export type RequiredKeys<T, K extends keyof T> = T & Required<Pick<T, K>>;
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};
