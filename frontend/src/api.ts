import axios from 'axios';
import { z } from 'zod';
import { API_BASE_URL } from './config';
import type { AssetDto, TradeMode, TradeResultDto, WalletDto } from './types';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

export const tradeSchema = z.object({
  userId: z.string().uuid(),
  symbol: z.string().min(1),
  amount: z.number().positive(),
  requestedPrice: z.number().positive()
});

export type TradeRequest = z.infer<typeof tradeSchema>;

export async function getWallet(userId: string): Promise<WalletDto> {
  const response = await api.get<WalletDto>(`/api/wallet/${userId}`);
  return response.data;
}

export async function getAssets(userId: string): Promise<AssetDto[]> {
  const response = await api.get<AssetDto[]>(`/api/assets/${userId}`);
  return response.data;
}

export async function executeTrade(mode: TradeMode, request: TradeRequest): Promise<TradeResultDto> {
  const body = tradeSchema.parse(request);
  const endpoint = mode === 'buy' ? '/api/trade/buy' : '/api/trade/sell';
  const response = await api.post<TradeResultDto>(endpoint, body);
  return response.data;
}
