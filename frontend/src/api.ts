import axios from 'axios';
import { z } from 'zod';
import { API_BASE_URL } from './config';
import type { AuthResultDto, MarketTickerDto, OrderDto, OrderResultDto, PortfolioHistoryPointDto, PortfolioSummaryDto, TradeMode, TradeResultDto, TransactionDto, UserDto, WalletDto } from './types';

const TOKEN_KEY = 'tradeturk_token';
const REFRESH_TOKEN_KEY = 'tradeturk_refresh_token';

export const tokenStore = {
  get: () => sessionStorage.getItem(TOKEN_KEY),
  refresh: () => sessionStorage.getItem(REFRESH_TOKEN_KEY),
  set: (token: string, refreshToken?: string) => {
    sessionStorage.setItem(TOKEN_KEY, token);
    if (refreshToken) sessionStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clear: () => {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
  }
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
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original?._retry && tokenStore.refresh()) {
      original._retry = true;
      try {
        const refreshed = await axios.post<AuthResultDto>(`${API_BASE_URL}/api/auth/refresh`, { refreshToken: tokenStore.refresh() });
        tokenStore.set(refreshed.data.token, refreshed.data.refreshToken);
        original.headers.Authorization = `Bearer ${refreshed.data.token}`;
        return api(original);
      } catch {
        tokenStore.clear();
      }
    }

    if (!error.response) {
      return Promise.reject(new Error('API sunucusuna ulasilamiyor. Lutfen backend servisinin calistigindan emin olun.'));
    }

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

function parseOrThrow<T>(schema: z.ZodType<T>, request: unknown, message: string): T {
  const result = schema.safeParse(request);
  if (!result.success) {
    throw new Error(message);
  }

  return result.data;
}

export async function register(request: RegisterRequest): Promise<AuthResultDto> {
  const body = parseOrThrow(registerSchema, request, 'Lutfen ad soyad, gecerli email, en az 3 karakter kullanici adi ve en az 8 karakter sifre girin.');
  const response = await api.post<AuthResultDto>('/api/auth/register', body);
  tokenStore.set(response.data.token, response.data.refreshToken);
  return response.data;
}

export async function login(request: LoginRequest): Promise<AuthResultDto> {
  const body = parseOrThrow(loginSchema, request, 'Lutfen email/kullanici adi ve sifrenizi kontrol edin.');
  const response = await api.post<AuthResultDto>('/api/auth/login', body);
  tokenStore.set(response.data.token, response.data.refreshToken);
  return response.data;
}

export async function logout(): Promise<void> {
  const refreshToken = tokenStore.refresh();
  if (refreshToken) {
    await api.post('/api/auth/logout', { refreshToken });
  }
  tokenStore.clear();
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

export async function getPortfolioHistory(range = '7D'): Promise<PortfolioHistoryPointDto[]> {
  const response = await api.get<PortfolioHistoryPointDto[]>('/api/portfolio/history/me', { params: { range } });
  return response.data;
}

export async function getMarketSymbols(): Promise<string[]> {
  const response = await api.get<string[]>('/api/markets/symbols');
  return response.data;
}

export async function getMarketTickers(): Promise<MarketTickerDto[]> {
  const response = await api.get<MarketTickerDto[]>('/api/markets/tickers');
  return response.data;
}

export async function executeTrade(mode: TradeMode, request: TradeRequest): Promise<TradeResultDto> {
  const endpoint = mode === 'buy' ? '/api/trade/buy' : '/api/trade/sell';
  const body = parseOrThrow(tradeSchema, request, 'Lutfen gecerli sembol, miktar ve fiyat girin.');
  const response = await api.post<TradeResultDto>(endpoint, body);
  return response.data;
}

export async function createOrder(request: { symbol: string; side: 'BUY' | 'SELL'; type: string; quantity: number; price?: number; triggerPrice?: number }): Promise<OrderResultDto> {
  const response = await api.post<OrderResultDto>('/api/orders', request);
  return response.data;
}

export async function getOpenOrders(): Promise<OrderDto[]> {
  const response = await api.get<OrderDto[]>('/api/orders/open');
  return response.data;
}

export async function getOrderHistory(): Promise<OrderDto[]> {
  const response = await api.get<OrderDto[]>('/api/orders/history');
  return response.data;
}

export async function cancelOrder(id: string): Promise<OrderResultDto> {
  const response = await api.delete<OrderResultDto>(`/api/orders/${id}`);
  return response.data;
}
