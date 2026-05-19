export type TradeMode = 'buy' | 'sell';
export type Page = 'dashboard' | 'wallet' | 'transactions' | 'settings';

export type PriceState = {
  symbol: string;
  price: number;
  previousPrice: number;
  changePercent24h?: number;
  high24h?: number;
  low24h?: number;
  volume24h?: number;
};

export type UserDto = {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  preferredCurrency: string;
  themePreference: string;
};

export type AuthResultDto = {
  token: string;
  refreshToken: string;
  user: UserDto;
};

export type CardDto = {
  cardHolderName: string;
  maskedCardNumber: string;
  expiryMonth: number;
  expiryYear: number;
};

export type WalletDto = {
  id: string;
  totalBalance: number;
  availableBalance: number;
  virtualCard?: CardDto | null;
  assets: AssetDto[];
  portfolioTotalValue: number;
};

export type AssetDto = {
  id: string;
  userId: string;
  walletId: string;
  symbol: string;
  amount: number;
  averageCost: number;
};

export type TransactionDto = {
  id: string;
  type: 'BUY' | 'SELL';
  symbol: string;
  amount: number;
  price: number;
  total: number;
  createdAt: string;
  status: string;
};

export type PortfolioSummaryDto = {
  totalPortfolioValue: number;
  availableUsdt: number;
  totalAssetValue: number;
  totalPnl: number;
  dailyPnl: number;
  weeklyPnl: number;
  unrealizedPnl: number;
  bestPerformer?: AssetAllocationDto | null;
  worstPerformer?: AssetAllocationDto | null;
  assetAllocation: AssetAllocationDto[];
};

export type AssetAllocationDto = {
  symbol: string;
  amount: number;
  averageCost: number;
  currentPrice: number;
  value: number;
  allocationPercent: number;
  unrealizedPnl: number;
};

export type TradeResultDto = {
  isSuccess: boolean;
  message: string;
  transactionId?: string;
  executedPrice: number;
  commissionUsed: number;
  slippageAmount: number;
};

export type MarketTickerDto = {
  symbol: string;
  price: number;
  changePercent24h: number;
  high24h: number;
  low24h: number;
  volume24h: number;
  retrievedAtUtc: string;
};

export type OrderDto = {
  id: string;
  symbol: string;
  side: 'BUY' | 'SELL';
  type: 'MARKET' | 'LIMIT' | 'STOP_LOSS' | 'TAKE_PROFIT';
  status: 'PENDING' | 'FILLED' | 'PARTIALLY_FILLED' | 'CANCELLED' | 'FAILED';
  quantity: number;
  price?: number | null;
  triggerPrice?: number | null;
  filledQuantity: number;
  averageFillPrice?: number | null;
  total: number;
  createdAt: string;
  filledAt?: string | null;
  cancelledAt?: string | null;
};

export type OrderResultDto = {
  isSuccess: boolean;
  message: string;
  order?: OrderDto | null;
};

export type PortfolioHistoryPointDto = {
  createdAt: string;
  totalValue: number;
  availableUSDT: number;
  assetValue: number;
  totalPnL: number;
};
