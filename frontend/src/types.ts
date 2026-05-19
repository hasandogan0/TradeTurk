export type TradeMode = 'buy' | 'sell';
export type Page = 'dashboard' | 'wallet' | 'transactions' | 'settings';

export type PriceState = {
  symbol: string;
  price: number;
  previousPrice: number;
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
  unrealizedPnl: number;
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
