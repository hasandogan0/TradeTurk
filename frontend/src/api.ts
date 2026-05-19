import axios from 'axios';
import { z } from 'zod';
import { API_BASE_URL } from './config';
import type { AuthResultDto, PortfolioSummaryDto, TradeMode, TradeResultDto, TransactionDto, UserDto, WalletDto } from './types';

const TOKEN_KEY = 'tradeturk_token';

export const tokenStore = {
  get: () => sessionStorage.getItem(TOKEN_KEY),
  set: (token: string) => sessionStorage.setItem(TOKEN_KEY, token),
  clear: () => sessionStorage.removeItem(TOKEN_KEY)
};

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
});

api.interceptors.request.use((config) => {
  const token = tokenStore.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.message ?? error.response?.data?.Message ?? error.message ?? 'Islem basarisiz oldu.';
    return Promise.reject(new Error(message));
  }
);

export const registerSchema = z.object({
  fullName: z.string().min(2),
  email: z.string().email(),
  userName: z.string().min(3),
  password: z.string().min(8)
});

export const loginSchema = z.object({
  emailOrUserName: z.string().min(3),
  password: z.string().min(1)
});

export const tradeSchema = z.object({
  symbol: z.string().min(1),
  amount: z.number().positive(),
  requestedPrice: z.number().positive()
});

export type RegisterRequest = z.infer<typeof registerSchema>;
export type LoginRequest = z.infer<typeof loginSchema>;
export type TradeRequest = z.infer<typeof tradeSchema>;

export async function register(request: RegisterRequest): Promise<AuthResultDto> {
  const response = await api.post<AuthResultDto>('/api/auth/register', registerSchema.parse(request));
  tokenStore.set(response.data.token);
  return response.data;
}

export async function login(request: LoginRequest): Promise<AuthResultDto> {
  const response = await api.post<AuthResultDto>('/api/auth/login', loginSchema.parse(request));
  tokenStore.set(response.data.token);
  return response.data;
}

export async function getMe(): Promise<UserDto> {
  const response = await api.get<UserDto>('/api/users/me');
  return response.data;
}

export async function updateMe(request: Omit<UserDto, 'id'>): Promise<UserDto> {
  const response = await api.put<UserDto>('/api/users/me', request);
  return response.data;
}

export async function changePassword(request: { currentPassword: string; newPassword: string }): Promise<{ message: string }> {
  const response = await api.put<{ message: string }>('/api/users/me/password', request);
  return response.data;
}

export async function getWallet(): Promise<WalletDto> {
  const response = await api.get<WalletDto>('/api/wallet/me');
  return response.data;
}

export async function getTransactions(): Promise<TransactionDto[]> {
  const response = await api.get<TransactionDto[]>('/api/transactions/me');
  return response.data;
}

export async function getPortfolioSummary(): Promise<PortfolioSummaryDto> {
  const response = await api.get<PortfolioSummaryDto>('/api/portfolio/summary/me');
  return response.data;
}

export async function executeTrade(mode: TradeMode, request: TradeRequest): Promise<TradeResultDto> {
  const endpoint = mode === 'buy' ? '/api/trade/buy' : '/api/trade/sell';
  const response = await api.post<TradeResultDto>(endpoint, tradeSchema.parse(request));
  return response.data;
}
