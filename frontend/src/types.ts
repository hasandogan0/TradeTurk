export type TradeMode = 'buy' | 'sell';

export type PriceState = {
  symbol: string;
  price: number;
  previousPrice: number;
};

export type WalletDto = {
  id: string;
  userId: string;
  fiatBalance: number;
};

export type AssetDto = {
  id: string;
  userId: string;
  walletId: string;
  symbol: string;
  amount: number;
  averageCost: number;
};

export type TradeResultDto = {
  isSuccess: boolean;
  message: string;
  transactionId?: string;
  executedPrice: number;
  commissionUsed: number;
  slippageAmount: number;
};
