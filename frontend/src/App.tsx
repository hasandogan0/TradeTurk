import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Activity,
  BarChart3,
  Bell,
  BellRing,
  CheckCircle2,
  CreditCard,
  History,
  LayoutDashboard,
  Loader2,
  LogOut,
  Menu,
  Search,
  Settings,
  ShieldCheck,
  Sparkles,
  Star,
  Wallet,
  Wifi,
  X,
  Inbox,
  AlertCircle
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { API_BASE_URL } from './config';
import {
  changePassword,
  cancelOrder,
  createOrder,
  executeTrade,
  getMarketTickers,
  getMe,
  getOpenOrders,
  getOrderHistory,
  getPortfolioHistory,
  getPortfolioSummary,
  getTransactions,
  getWallet,
  login,
  logout as logoutRequest,
  register,
  tokenStore,
  updateMe
} from './api';
import type { MarketTickerDto, OrderDto, Page, PriceState, TradeMode, UserDto } from './types';

const defaultSymbols = [
  'BTCUSDT', 'ETHUSDT', 'BNBUSDT', 'SOLUSDT', 'XRPUSDT',
  'ADAUSDT', 'DOGEUSDT', 'AVAXUSDT', 'DOTUSDT', 'LINKUSDT',
  'MATICUSDT', 'LTCUSDT', 'TRXUSDT', 'ATOMUSDT', 'NEARUSDT'
] as const;

const initialPrices: Record<string, PriceState> = Object.fromEntries(
  defaultSymbols.map((symbol) => [symbol, { symbol, price: 0, previousPrice: 0 }])
) as Record<string, PriceState>;

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: value > 100 ? 2 : 5 }).format(value || 0);
const formatNumber = (value: number) => new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value || 0);
const coinShort = (symbol: string) => symbol.replace('USDT', '');
const coinName = (symbol: string) => ({
  BTCUSDT: 'Bitcoin', ETHUSDT: 'Ethereum', BNBUSDT: 'BNB', SOLUSDT: 'Solana', XRPUSDT: 'XRP',
  ADAUSDT: 'Cardano', DOGEUSDT: 'Dogecoin', AVAXUSDT: 'Avalanche', DOTUSDT: 'Polkadot', LINKUSDT: 'Chainlink',
  MATICUSDT: 'Polygon', LTCUSDT: 'Litecoin', TRXUSDT: 'TRON', ATOMUSDT: 'Cosmos', NEARUSDT: 'NEAR'
}[symbol] ?? symbol);

export function App() {
  const queryClient = useQueryClient();
  const [token, setToken] = useState(() => tokenStore.get());
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [page, setPage] = useState<Page>('dashboard');
  const [mobileOpen, setMobileOpen] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const [prices, setPrices] = useState(initialPrices);
  const [connectionLabel, setConnectionLabel] = useState('Bağlanıyor...');
  const [mode, setMode] = useState<TradeMode>('buy');
  const [orderType, setOrderType] = useState<'MARKET' | 'LIMIT' | 'STOP_LOSS' | 'TAKE_PROFIT'>('MARKET');
  const [selectedSymbol, setSelectedSymbol] = useState('BTCUSDT');
  const [amount, setAmount] = useState('');
  const [limitPrice, setLimitPrice] = useState('');
  const [triggerPrice, setTriggerPrice] = useState('');
  const [historyRange, setHistoryRange] = useState('7D');
  const [search, setSearch] = useState('');
  const [favorites, setFavorites] = useState<string[]>(['BTCUSDT', 'ETHUSDT', 'SOLUSDT', 'BNBUSDT']);
  const [notice, setNotice] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, enabled: Boolean(token), retry: false });
  const walletQuery = useQuery({ queryKey: ['wallet'], queryFn: getWallet, enabled: Boolean(token) });
  const transactionsQuery = useQuery({ queryKey: ['transactions'], queryFn: getTransactions, enabled: Boolean(token) });
  const portfolioQuery = useQuery({ queryKey: ['portfolio-summary'], queryFn: getPortfolioSummary, enabled: Boolean(token) });
  const portfolioHistoryQuery = useQuery({ queryKey: ['portfolio-history', historyRange], queryFn: () => getPortfolioHistory(historyRange), enabled: Boolean(token) });
  const marketQuery = useQuery({ queryKey: ['market-tickers'], queryFn: getMarketTickers, enabled: Boolean(token), refetchInterval: 30000 });
  const openOrdersQuery = useQuery({ queryKey: ['orders-open'], queryFn: getOpenOrders, enabled: Boolean(token), refetchInterval: 15000 });
  const orderHistoryQuery = useQuery({ queryKey: ['orders-history'], queryFn: getOrderHistory, enabled: Boolean(token), refetchInterval: 30000 });

  useEffect(() => {
    if (meQuery.isError) {
      tokenStore.clear();
      setToken(null);
    }
  }, [meQuery.isError]);

  useEffect(() => {
    if (!marketQuery.data) return;
    setPrices((current) => {
      const next = { ...current };
      for (const ticker of marketQuery.data) {
        next[ticker.symbol] = {
          symbol: ticker.symbol,
          previousPrice: current[ticker.symbol]?.price ?? ticker.price,
          price: Number(ticker.price),
          changePercent24h: Number(ticker.changePercent24h),
          high24h: Number(ticker.high24h),
          low24h: Number(ticker.low24h),
          volume24h: Number(ticker.volume24h)
        };
      }
      return next;
    });
  }, [marketQuery.data]);

  useEffect(() => {
    const connection = new HubConnectionBuilder().withUrl(`${API_BASE_URL}/priceHub`).withAutomaticReconnect().build();
    connection.on('ReceivePriceUpdate', (symbol: string, price: number) => {
      setPrices((current) => ({
        ...current,
        [symbol]: { ...current[symbol], symbol, previousPrice: current[symbol]?.price ?? 0, price: Number(price) }
      }));
    });
    connection.on('ReceiveTickerUpdate', (ticker: MarketTickerDto) => {
      setPrices((current) => ({
        ...current,
        [ticker.symbol]: {
          symbol: ticker.symbol,
          previousPrice: current[ticker.symbol]?.price ?? ticker.price,
          price: Number(ticker.price),
          changePercent24h: Number(ticker.changePercent24h),
          high24h: Number(ticker.high24h),
          low24h: Number(ticker.low24h),
          volume24h: Number(ticker.volume24h)
        }
      }));
    });
    connection.start()
      .then(() => setConnectionLabel(connection.state === HubConnectionState.Connected ? 'Canlı Veri Bağlı' : 'Bağlanıyor...'))
      .catch(() => setConnectionLabel('Bağlantı Hatası'));
    return () => { void connection.stop(); };
  }, []);

  const selectedTicker = prices[selectedSymbol] ?? initialPrices.BTCUSDT;
  const selectedPrice = selectedTicker.price ?? 0;
  const numericAmount = Number(amount) || 0;
  const fee = numericAmount * selectedPrice * 0.001;
  const gross = numericAmount * selectedPrice;
  const total = mode === 'buy' ? gross + fee : gross - fee;
  const assets = walletQuery.data?.assets ?? [];
  const portfolioValue = portfolioQuery.data?.totalPortfolioValue ?? walletQuery.data?.portfolioTotalValue ?? 0;

  const visibleSymbols = useMemo(() => {
    const query = search.trim().toUpperCase();
    return defaultSymbols.filter((symbol) => !query || symbol.includes(query) || coinName(symbol).toUpperCase().includes(query));
  }, [search]);

  const tradeMutation = useMutation({
    mutationFn: () => orderType === 'MARKET'
      ? createOrder({ symbol: selectedSymbol, side: mode.toUpperCase() as 'BUY' | 'SELL', type: orderType, quantity: numericAmount, price: selectedPrice })
      : createOrder({
        symbol: selectedSymbol,
        side: mode.toUpperCase() as 'BUY' | 'SELL',
        type: orderType,
        quantity: numericAmount,
        price: Number(limitPrice) || selectedPrice,
        triggerPrice: Number(triggerPrice) || Number(limitPrice) || selectedPrice
      }),
    onSuccess: async (result) => {
      setNotice({ type: result.isSuccess ? 'success' : 'error', message: result.message });
      setAmount('');
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['wallet'] }),
        queryClient.invalidateQueries({ queryKey: ['transactions'] }),
        queryClient.invalidateQueries({ queryKey: ['portfolio-summary'] }),
        queryClient.invalidateQueries({ queryKey: ['orders-open'] }),
        queryClient.invalidateQueries({ queryKey: ['orders-history'] }),
        queryClient.invalidateQueries({ queryKey: ['portfolio-history'] })
      ]);
    },
    onError: (error) => setNotice({ type: 'error', message: error instanceof Error ? error.message : 'Islem basarisiz oldu.' })
  });

  const cancelMutation = useMutation({
    mutationFn: cancelOrder,
    onSuccess: async (result) => {
      setNotice({ type: result.isSuccess ? 'success' : 'error', message: result.message });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['orders-open'] }),
        queryClient.invalidateQueries({ queryKey: ['orders-history'] })
      ]);
    },
    onError: (error) => setNotice({ type: 'error', message: error instanceof Error ? error.message : 'Emir iptal edilemedi.' })
  });

  async function logout() {
    try {
      await logoutRequest();
    } catch {
      tokenStore.clear();
    }
    setToken(null);
    queryClient.clear();
  }

  function submitTrade() {
    setNotice(null);
    if (!numericAmount || numericAmount <= 0) return setNotice({ type: 'error', message: 'Lütfen geçerli bir miktar girin.' });
    if (!selectedPrice || selectedPrice <= 0) return setNotice({ type: 'error', message: 'Canlı fiyat henüz hazır değil.' });
    if (orderType === 'LIMIT' && (!Number(limitPrice) || Number(limitPrice) <= 0)) return setNotice({ type: 'error', message: 'Limit fiyat girin.' });
    if ((orderType === 'STOP_LOSS' || orderType === 'TAKE_PROFIT') && (!Number(triggerPrice) || Number(triggerPrice) <= 0)) return setNotice({ type: 'error', message: 'Trigger fiyat girin.' });
    tradeMutation.mutate();
  }

  function toggleFavorite(symbol: string) {
    setFavorites((current) => current.includes(symbol) ? current.filter((item) => item !== symbol) : [...current, symbol]);
  }

  if (!token) {
    return <AuthScreen authMode={authMode} setAuthMode={setAuthMode} onToken={(nextToken) => setToken(nextToken)} />;
  }

  const nav = [
    [LayoutDashboard, 'Dashboard', 'dashboard'],
    [Wallet, 'Cüzdanım', 'wallet'],
    [History, 'Geçmiş', 'transactions'],
    [Settings, 'Ayarlar', 'settings']
  ] as const;

  return (
    <div className="min-h-screen bg-ink text-base text-slate-100 selection:bg-accent/30">
      {notice && <Toast type={notice.type} message={notice.message} onClose={() => setNotice(null)} />}
      <aside className="fixed inset-y-0 left-0 hidden w-72 border-r border-line bg-panel/95 px-6 py-8 backdrop-blur-2xl lg:block">
        <Brand />
        <nav className="mt-10 space-y-2">
          {nav.map(([Icon, label, target]) => (
            <button key={target} onClick={() => setPage(target)} className={`flex w-full items-center gap-4 rounded-2xl px-5 py-4 text-left text-base font-bold transition-all duration-200 ${page === target ? 'bg-accent/15 text-accent shadow-xl shadow-accent/5' : 'text-slate-400 hover:bg-white/[0.06] hover:text-slate-200'}`}>
              <Icon size={22} />
              <span>{label}</span>
            </button>
          ))}
        </nav>
        <UserPanel user={meQuery.data} onLogout={logout} />
      </aside>

      {mobileOpen && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm lg:hidden animate-fadeIn">
          <div className="h-full w-4/5 max-w-[320px] bg-panel p-6 shadow-2xl">
            <button className="mb-8 grid min-h-[44px] min-w-[44px] place-items-center rounded-xl border border-line bg-white/[0.04] transition hover:bg-white/10" onClick={() => setMobileOpen(false)}><X size={24} /></button>
            <Brand />
            <nav className="mt-10 space-y-3">
              {nav.map(([Icon, label, target]) => (
                <button key={target} onClick={() => { setPage(target); setMobileOpen(false); }} className={`flex w-full items-center gap-4 rounded-xl px-4 py-4 text-left text-lg font-bold transition-colors ${page === target ? 'bg-accent/15 text-accent' : 'text-slate-300 hover:bg-white/[0.06]'}`}>
                  <Icon size={24} /> {label}
                </button>
              ))}
            </nav>
          </div>
        </div>
      )}

      <main className="lg:pl-72 flex flex-col min-h-screen pb-[80px] lg:pb-0">
        <header className="sticky top-0 z-30 border-b border-line bg-ink/90 px-4 py-4 backdrop-blur-xl lg:px-8">
          <div className="flex items-center justify-between gap-4">
            <button className="grid min-h-[44px] min-w-[44px] place-items-center rounded-xl border border-line bg-white/[0.03] lg:hidden transition hover:bg-white/[0.08]" onClick={() => setMobileOpen(true)}><Menu size={22} /></button>
            <div className="flex min-w-0 flex-1 items-center gap-3 rounded-2xl border border-line bg-white/[0.04] px-4 py-3 transition-colors focus-within:border-accent/50 focus-within:bg-white/[0.06]">
              <Search size={20} className="text-slate-500" />
              <input value={search} onChange={(event) => setSearch(event.target.value)} className="w-full bg-transparent text-base font-medium text-white outline-none placeholder:text-slate-500" placeholder="Sembol Ara: BTC, SOL..." />
            </div>
            <div className="hidden items-center gap-2 rounded-2xl border border-line bg-white/[0.04] px-5 py-3 text-sm font-bold text-slate-300 sm:flex">
              {connectionLabel.includes('Canlı') ? <Wifi size={18} className="text-accent" /> : <Loader2 size={18} className="animate-spin text-amber-400" />}
              {connectionLabel}
            </div>
            <div className="relative">
              <button onClick={() => setShowNotifications(!showNotifications)} className="grid min-h-[44px] min-w-[44px] place-items-center rounded-xl border border-line bg-white/[0.04] transition hover:bg-white/10">
                <Bell size={20} />
              </button>
              {showNotifications && (
                <div className="absolute right-0 top-full mt-3 w-[320px] rounded-2xl border border-line bg-panel p-5 shadow-2xl backdrop-blur-xl animate-slideUp">
                  <div className="flex items-center justify-between mb-4 border-b border-line pb-3">
                    <h3 className="text-lg font-black text-white">Bildirimler</h3>
                    <button onClick={() => setShowNotifications(false)} className="text-slate-400 hover:text-white transition"><X size={20} /></button>
                  </div>
                  <div className="flex flex-col items-center justify-center py-8 text-slate-400">
                    <BellRing size={36} className="mb-4 opacity-20" />
                    <p className="text-base font-medium">Henüz yeni bildirim yok.</p>
                  </div>
                </div>
              )}
            </div>
          </div>
          <TickerStrip symbols={defaultSymbols} prices={prices} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} />
        </header>

        <div className="flex-1 px-4 pt-6 pb-12 lg:px-8 max-w-[1600px] w-full mx-auto">
          {page === 'dashboard' && (
            <Dashboard
              prices={prices}
              summary={portfolioQuery.data}
              walletBalance={walletQuery.data?.availableBalance ?? 0}
              portfolioValue={portfolioValue}
              mode={mode}
              setMode={setMode}
              orderType={orderType}
              setOrderType={setOrderType}
              selectedSymbol={selectedSymbol}
              setSelectedSymbol={setSelectedSymbol}
              amount={amount}
              setAmount={setAmount}
              limitPrice={limitPrice}
              setLimitPrice={setLimitPrice}
              triggerPrice={triggerPrice}
              setTriggerPrice={setTriggerPrice}
              selectedTicker={selectedTicker}
              selectedPrice={selectedPrice}
              fee={fee}
              total={total}
              submitTrade={submitTrade}
              isTrading={tradeMutation.isPending}
              assets={assets}
              symbols={visibleSymbols}
              favorites={favorites}
              toggleFavorite={toggleFavorite}
              transactions={transactionsQuery.data ?? []}
              openOrders={openOrdersQuery.data ?? []}
              orderHistory={orderHistoryQuery.data ?? []}
              onCancelOrder={(id: string) => cancelMutation.mutate(id)}
              portfolioHistory={portfolioHistoryQuery.data ?? []}
              historyRange={historyRange}
              setHistoryRange={setHistoryRange}
              isMarketLoading={marketQuery.isLoading}
            />
          )}
          {page === 'wallet' && <WalletPage isLoading={walletQuery.isLoading} isError={walletQuery.isError} wallet={walletQuery.data} prices={prices} />}
          {page === 'transactions' && <TransactionsPage isLoading={transactionsQuery.isLoading} transactions={transactionsQuery.data ?? []} />}
          {page === 'settings' && <SettingsPage user={meQuery.data} onSaved={(user) => queryClient.setQueryData(['me'], user)} setNotice={setNotice} />}
        </div>
      </main>
      
      <nav className="fixed inset-x-0 bottom-0 z-40 grid grid-cols-4 border-t border-line bg-panel/95 p-2 backdrop-blur-xl lg:hidden safe-area-pb">
        {nav.map(([Icon, label, target]) => (
          <button key={target} onClick={() => setPage(target)} className={`flex min-h-[56px] flex-col items-center justify-center rounded-xl text-xs font-black transition-colors ${page === target ? 'bg-accent/15 text-accent' : 'text-slate-400 hover:bg-white/[0.04]'}`}>
            <Icon size={22} className="mb-1" />
            <span>{label}</span>
          </button>
        ))}
      </nav>
    </div>
  );
}

function AuthScreen({ authMode, setAuthMode, onToken }: { authMode: 'login' | 'register'; setAuthMode: (mode: 'login' | 'register') => void; onToken: (token: string) => void }) {
  const [form, setForm] = useState({ fullName: '', email: '', userName: '', emailOrUserName: '', password: '' });
  const [error, setError] = useState('');
  const mutation = useMutation({
    mutationFn: () => authMode === 'login' ? login({ emailOrUserName: form.emailOrUserName, password: form.password }) : register({ fullName: form.fullName, email: form.email, userName: form.userName, password: form.password }),
    onSuccess: (result) => onToken(result.token),
    onError: (err) => setError(err instanceof Error ? err.message : 'Giriş yapılamadı.')
  });

  return (
    <main className="grid min-h-screen place-items-center bg-ink px-4 py-10 text-slate-100">
      <section className="w-full max-w-xl rounded-[2rem] border border-line bg-panel/80 p-8 sm:p-12 shadow-2xl shadow-cyan-500/10 backdrop-blur-2xl animate-fadeIn">
        <Brand />
        <h1 className="mt-10 text-3xl sm:text-4xl font-black text-white tracking-tight">{authMode === 'login' ? 'TradeTurk\'e Giriş Yap' : 'Demo Portföyünü Oluştur'}</h1>
        <p className="mt-4 text-base lg:text-lg font-medium text-slate-400">{authMode === 'login' ? 'Premium trading terminaline devam et.' : '50,000 USDT demo cüzdan ve sanal kart anında hazır.'}</p>
        
        {error && <div className="mt-6 flex items-center gap-3 rounded-2xl border border-danger/30 bg-danger/10 p-5 text-base font-bold text-rose-200 animate-slideUp"><AlertCircle size={20} className="text-danger flex-shrink-0" />{error}</div>}
        
        <div className="mt-8 space-y-5">
          {authMode === 'register' && <Input label="Ad Soyad" value={form.fullName} onChange={(fullName) => setForm({ ...form, fullName })} />}
          {authMode === 'register' && <Input label="Email" type="email" value={form.email} onChange={(email) => setForm({ ...form, email })} />}
          {authMode === 'register' && <Input label="Kullanıcı Adı" value={form.userName} onChange={(userName) => setForm({ ...form, userName })} />}
          {authMode === 'login' && <Input label="Email veya Kullanıcı Adı" value={form.emailOrUserName} onChange={(emailOrUserName) => setForm({ ...form, emailOrUserName })} />}
          <Input label="Şifre" type="password" value={form.password} onChange={(password) => setForm({ ...form, password })} />
          
          <button disabled={mutation.isPending} onClick={() => mutation.mutate()} className="mt-2 flex w-full min-h-[56px] items-center justify-center gap-3 rounded-2xl bg-accent px-6 py-4 text-lg font-black text-white shadow-xl shadow-accent/20 transition hover:bg-accent/90 disabled:opacity-70">
            {mutation.isPending && <Loader2 className="animate-spin" size={24} />} {authMode === 'login' ? 'Giriş Yap' : 'Kayıt Ol ve Başla'}
          </button>
          <button className="w-full py-4 text-base font-bold text-cyan-400 transition hover:text-cyan-300" onClick={() => { setError(''); setAuthMode(authMode === 'login' ? 'register' : 'login'); }}>
            {authMode === 'login' ? 'Yeni hesap oluştur' : 'Zaten hesabım var, giriş yap'}
          </button>
        </div>
      </section>
    </main>
  );
}

function Dashboard(props: any) {
  const {
    prices,
    summary,
    walletBalance,
    portfolioValue,
    mode,
    setMode,
    orderType,
    setOrderType,
    selectedSymbol,
    setSelectedSymbol,
    amount,
    setAmount,
    limitPrice,
    setLimitPrice,
    triggerPrice,
    setTriggerPrice,
    selectedTicker,
    selectedPrice,
    fee,
    total,
    submitTrade,
    isTrading,
    assets,
    symbols,
    favorites,
    toggleFavorite,
    transactions,
    openOrders,
    orderHistory,
    onCancelOrder,
    portfolioHistory,
    historyRange,
    setHistoryRange,
    isMarketLoading
  } = props;

  return (
    <div className="space-y-6 lg:space-y-8">
      <section className="grid gap-4 sm:gap-6 grid-cols-1 sm:grid-cols-2 xl:grid-cols-4">
        <Metric title="Toplam Portföy" value={formatCurrency(portfolioValue)} sub="Canlı Varlık Değeri" />
        <Metric title="Kullanılabilir USDT" value={formatCurrency(walletBalance)} sub="Alım Gücü" />
        <Metric title="Günlük K/Z" value={formatCurrency(summary?.dailyPnl ?? 0)} sub="24 Saatlik Performans" />
        <Metric title="Haftalık K/Z" value={formatCurrency(summary?.weeklyPnl ?? 0)} sub="7 Günlük Trend" />
      </section>

      <section className="grid gap-6 xl:grid-cols-[320px_1fr_360px] lg:grid-cols-[1fr_360px]">
        <div className="hidden xl:block">
          <Watchlist symbols={symbols} prices={prices} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} favorites={favorites} toggleFavorite={toggleFavorite} isLoading={isMarketLoading} />
        </div>
        <div className="flex flex-col gap-6">
          <ChartPanel symbol={selectedSymbol} ticker={selectedTicker} />
          <div className="xl:hidden">
            <Watchlist symbols={symbols} prices={prices} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} favorites={favorites} toggleFavorite={toggleFavorite} isLoading={isMarketLoading} horizontal />
          </div>
        </div>
        <TradePanel mode={mode} setMode={setMode} orderType={orderType} setOrderType={setOrderType} selectedSymbol={selectedSymbol} amount={amount} setAmount={setAmount} limitPrice={limitPrice} setLimitPrice={setLimitPrice} triggerPrice={triggerPrice} setTriggerPrice={setTriggerPrice} selectedPrice={selectedPrice} fee={fee} total={total} submitTrade={submitTrade} isTrading={isTrading} />
      </section>

      <section className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <PortfolioAnalytics summary={summary} assets={assets} prices={prices} />
        <AiInsight summary={summary} assets={assets} />
      </section>

      <PortfolioHistoryChart points={portfolioHistory} range={historyRange} setRange={setHistoryRange} />

      <OrderTables openOrders={openOrders} orderHistory={orderHistory} onCancel={onCancelOrder} />

      <section className="grid gap-6 xl:grid-cols-[1fr_420px]">
        <RecentTrades transactions={transactions} />
        <AssetList assets={assets} prices={prices} />
      </section>
    </div>
  );
}

function TickerStrip({ symbols, prices, selectedSymbol, setSelectedSymbol }: any) {
  return (
    <div className="mt-5 flex gap-3 overflow-x-auto pb-3 snap-x snap-mandatory hide-scrollbar">
      {symbols.map((symbol: string) => {
        const ticker = prices[symbol] ?? {};
        const positive = (ticker.changePercent24h ?? 0) >= 0;
        return (
          <button key={symbol} onClick={() => setSelectedSymbol(symbol)} className={`min-w-[160px] sm:min-w-[180px] snap-start rounded-2xl border px-5 py-4 text-left transition-all duration-300 hover:-translate-y-1 ${selectedSymbol === symbol ? 'border-accent/60 bg-accent/10 shadow-lg shadow-accent/10' : 'border-line bg-panel/60 hover:bg-white/[0.06]'}`}>
            <div className="flex items-center justify-between text-sm sm:text-base font-black">
              <span className="text-slate-200">{coinShort(symbol)}</span>
              <span className={positive ? 'text-accent' : 'text-danger'}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</span>
            </div>
            <div className="mt-2 text-xl sm:text-2xl font-black text-white tracking-tight">{formatCurrency(ticker.price ?? 0)}</div>
          </button>
        );
      })}
    </div>
  );
}

function Watchlist({ symbols, prices, selectedSymbol, setSelectedSymbol, favorites, toggleFavorite, isLoading, horizontal }: any) {
  return (
    <section className={`flex flex-col rounded-2xl border border-line bg-panel/95 shadow-2xl overflow-hidden ${horizontal ? '' : 'h-full max-h-[700px]'}`}>
      <div className="p-5 border-b border-line bg-white/[0.02] flex items-center justify-between">
        <h2 className="text-lg font-black text-white flex items-center gap-2"><Activity size={20} className="text-accent" /> Canlı Piyasa</h2>
        {isLoading && <Loader2 className="animate-spin text-slate-400" size={20} />}
      </div>
      <div className={`p-3 space-y-2 overflow-auto ${horizontal ? 'flex gap-3 overflow-x-auto pb-4 snap-x' : 'flex-1 overflow-y-auto'}`}>
        {symbols.length === 0 && !isLoading && <State text="Sonuç bulunamadı." icon={Search} />}
        {symbols.map((symbol: string) => {
          const ticker = prices[symbol] ?? {};
          const positive = (ticker.changePercent24h ?? 0) >= 0;
          const favorite = favorites.includes(symbol);
          return (
            <button key={symbol} onClick={() => setSelectedSymbol(symbol)} className={`group ${horizontal ? 'min-w-[220px] snap-start flex-shrink-0' : 'w-full'} grid grid-cols-[1fr_auto] gap-3 rounded-2xl border p-4 text-left transition-all duration-200 ${selectedSymbol === symbol ? 'border-accent/50 bg-accent/10' : 'border-transparent bg-white/[0.02] hover:bg-white/[0.06] hover:border-white/10'}`}>
              <div>
                <div className="flex items-center gap-2">
                  <button type="button" onClick={(event) => { event.stopPropagation(); toggleFavorite(symbol); }} className={`p-1.5 -ml-1.5 transition-colors rounded-lg ${favorite ? 'text-amber-400' : 'text-slate-600 hover:text-amber-400 hover:bg-white/5'}`}><Star size={18} fill={favorite ? 'currentColor' : 'none'} /></button>
                  <span className="text-base sm:text-lg font-black text-slate-100 tracking-tight">{coinShort(symbol)}</span>
                </div>
                <div className="mt-1 text-sm font-medium text-slate-500 pl-1">{coinName(symbol)}</div>
              </div>
              <div className="text-right">
                <div className="text-base sm:text-lg font-black text-white">{formatCurrency(ticker.price ?? 0)}</div>
                <div className={`mt-1 text-sm font-bold ${positive ? 'text-accent' : 'text-danger'}`}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</div>
              </div>
            </button>
          );
        })}
      </div>
    </section>
  );
}

function ChartPanel({ symbol, ticker }: { symbol: string; ticker: PriceState }) {
  const tradingViewSymbol = `BINANCE:${symbol}`;
  const chartUrl = `https://s.tradingview.com/widgetembed/?symbol=${encodeURIComponent(tradingViewSymbol)}&interval=60&theme=dark&style=1&timezone=Etc%2FUTC&hideideas=1&studies=[]&withdateranges=1&saveimage=0`;
  const positive = (ticker.changePercent24h ?? 0) >= 0;

  return (
    <section className="flex flex-col overflow-hidden rounded-2xl border border-line bg-panel shadow-2xl">
      <div className="flex flex-col sm:flex-row flex-wrap sm:items-center justify-between gap-5 border-b border-line p-5 sm:p-6 bg-white/[0.02]">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl sm:text-3xl font-black text-white tracking-tight">{coinShort(symbol)} / USDT</h1>
            <span className={`rounded-full px-3 py-1 text-sm sm:text-base font-black ${positive ? 'bg-accent/15 text-accent' : 'bg-danger/15 text-danger'}`}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</span>
          </div>
          <div className="mt-2 text-3xl sm:text-4xl font-black text-white">{formatCurrency(ticker.price ?? 0)}</div>
        </div>
        <div className="grid grid-cols-3 gap-3 sm:gap-6 text-right bg-black/20 p-4 rounded-xl border border-line w-full sm:w-auto">
          <MiniStat label="24h Yüksek" value={formatCurrency(ticker.high24h ?? 0)} />
          <MiniStat label="24h Düşük" value={formatCurrency(ticker.low24h ?? 0)} />
          <MiniStat label="Hacim" value={formatNumber(ticker.volume24h ?? 0)} />
        </div>
      </div>
      <div className="h-[350px] sm:h-[450px] lg:h-[500px] w-full bg-[#070914] relative">
        <iframe title="TradingView Chart" src={chartUrl} className="absolute inset-0 h-full w-full border-0" />
      </div>
      <div className="flex gap-2 border-t border-line p-3 sm:p-4 overflow-x-auto bg-white/[0.02] hide-scrollbar">
        {['1m', '5m', '15m', '1h', '4h', '1d', '1w'].map((item) => <button key={item} className="rounded-xl border border-line bg-white/[0.04] px-5 py-2.5 min-h-[44px] text-sm font-bold text-slate-300 transition hover:bg-white/10 hover:text-white focus:bg-white/10">{item}</button>)}
      </div>
    </section>
  );
}

function TradeInput({ label, value, onChange, currency, type = "number" }: any) {
  return (
    <div className="mb-5">
      <label className="mb-2 block text-xs sm:text-sm font-bold text-slate-400 uppercase tracking-wider">{label}</label>
      <div className="flex items-center rounded-2xl border border-line bg-panelSoft px-4 transition-colors focus-within:border-cyan-500/50 focus-within:bg-white/[0.04]">
        <input type={type} value={value} onChange={(e) => onChange(e.target.value)} min="0" step="0.00000001" className="w-full bg-transparent py-4 text-lg font-bold text-white outline-none" placeholder="0.00" />
        <span className="text-base font-black text-slate-500 pl-4 border-l border-line/50">{currency}</span>
      </div>
    </div>
  );
}

function TradePanel({ mode, setMode, orderType, setOrderType, selectedSymbol, amount, setAmount, limitPrice, setLimitPrice, triggerPrice, setTriggerPrice, selectedPrice, fee, total, submitTrade, isTrading }: any) {
  return (
    <section className="rounded-2xl border border-line bg-panel/95 p-5 sm:p-6 shadow-2xl xl:sticky xl:top-32 h-fit animate-slideUp">
      <div className="mb-6 flex items-center justify-between border-b border-line pb-4">
        <h2 className="text-xl font-black text-white">İşlem Emri</h2>
        {isTrading && <Loader2 className="animate-spin text-accent" size={20} />}
      </div>
      
      <div className="mb-6 grid grid-cols-2 gap-2 rounded-2xl bg-black/40 p-2 border border-line">
        {(['buy', 'sell'] as TradeMode[]).map((item) => (
          <button key={item} onClick={() => setMode(item)} className={`rounded-xl py-3.5 text-base font-black transition-all duration-300 ${mode === item ? (item === 'buy' ? 'bg-accent text-white shadow-lg shadow-accent/25' : 'bg-danger text-white shadow-lg shadow-danger/25') : 'text-slate-400 hover:bg-white/5 hover:text-slate-200'}`}>
            {item === 'buy' ? 'Alış (Long)' : 'Satış (Short)'}
          </button>
        ))}
      </div>

      <div className="mb-6">
        <label className="mb-2 block text-xs sm:text-sm font-bold text-slate-400 uppercase tracking-wider">İşlem Çifti</label>
        <div className="flex items-center justify-between rounded-2xl border border-line bg-white/[0.02] p-4">
          <span className="text-xl font-black text-white">{coinShort(selectedSymbol)}<span className="text-slate-500 text-base ml-1.5">/ USDT</span></span>
          <span className="text-lg font-bold text-slate-300">{formatCurrency(selectedPrice)}</span>
        </div>
      </div>

      <div className="mb-6">
        <label className="mb-2 block text-xs sm:text-sm font-bold text-slate-400 uppercase tracking-wider">Emir Tipi</label>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          {[ { id: 'MARKET', label: 'Market' }, { id: 'LIMIT', label: 'Limit' }, { id: 'STOP_LOSS', label: 'Stop' }, { id: 'TAKE_PROFIT', label: 'Target' } ].map((opt) => (
             <button key={opt.id} onClick={() => setOrderType(opt.id as any)} className={`min-h-[44px] rounded-xl border text-sm font-bold transition-all ${orderType === opt.id ? 'border-cyan-500/50 bg-cyan-500/10 text-cyan-300' : 'border-line bg-panelSoft text-slate-400 hover:bg-white/10 hover:text-slate-200'}`}>{opt.label}</button>
          ))}
        </div>
      </div>

      <TradeInput label="Miktar" value={amount} onChange={setAmount} currency={coinShort(selectedSymbol)} />
      
      {orderType === 'LIMIT' && <TradeInput label="Limit Fiyat" value={limitPrice} onChange={setLimitPrice} currency="USDT" />}
      {(orderType === 'STOP_LOSS' || orderType === 'TAKE_PROFIT') && <TradeInput label="Tetikleyici Fiyat" value={triggerPrice} onChange={setTriggerPrice} currency="USDT" />}
      
      <div className="mb-6 space-y-3.5 rounded-2xl bg-black/30 border border-line/50 p-5 text-base">
        <Row label="Birim Fiyat" value={formatCurrency(selectedPrice)} />
        <Row label="Komisyon (%0.1)" value={formatCurrency(fee)} />
        <Row label="Tahmini Toplam" value={formatCurrency(total)} strong />
      </div>
      
      <button disabled={isTrading} onClick={submitTrade} className={`w-full min-h-[56px] rounded-2xl py-4 text-lg font-black text-white transition-all disabled:opacity-60 disabled:cursor-not-allowed ${mode === 'buy' ? 'bg-accent shadow-xl shadow-accent/20 hover:bg-accent/90' : 'bg-danger shadow-xl shadow-danger/20 hover:bg-danger/90'}`}>
        {isTrading ? <Loader2 className="mx-auto animate-spin" size={24} /> : (orderType === 'MARKET' ? (mode === 'buy' ? 'Market Alımını Onayla' : 'Market Satışını Onayla') : 'Emri Deftere Gönder')}
      </button>
    </section>
  );
}

function PortfolioHistoryChart({ points, range, setRange }: any) {
  const data = points?.length ? points : [];
  const max = Math.max(...data.map((p: any) => Number(p.totalValue) || 0), 1);
  const min = Math.min(...data.map((p: any) => Number(p.totalValue) || max), max);
  const span = Math.max(max - min, 1);

  return (
    <section className="rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-2xl">
      <div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h2 className="text-xl sm:text-2xl font-black text-white">Portföy Performansı</h2>
          <p className="mt-1.5 text-sm sm:text-base text-slate-400">Snapshot equity curve ve PnL trendi.</p>
        </div>
        <div className="grid grid-cols-5 rounded-xl bg-black/30 p-1 border border-line overflow-hidden w-full sm:w-auto">
          {['1D', '7D', '1M', '3M', '1Y'].map((item) => <button key={item} onClick={() => setRange(item)} className={`min-h-[40px] px-3 sm:px-4 text-xs sm:text-sm font-black transition-colors rounded-lg ${range === item ? 'bg-white/10 text-white' : 'text-slate-500 hover:bg-white/5'}`}>{item}</button>)}
        </div>
      </div>
      {data.length === 0 ? <State text="Snapshot verisi bekleniyor. Grafik yakında oluşacak." icon={BarChart3} /> : (
        <div className="flex h-[250px] sm:h-72 items-end gap-1.5 sm:gap-2 rounded-2xl bg-black/20 p-4 border border-white/[0.02]">
          {data.map((point: any) => {
            const height = 12 + ((Number(point.totalValue) - min) / span) * 88;
            return <div key={point.createdAt} title={`${new Date(point.createdAt).toLocaleString()} ${formatCurrency(point.totalValue)}`} className="flex min-w-[6px] sm:min-w-[12px] flex-1 flex-col justify-end group"><div className="rounded-t-[4px] sm:rounded-t-lg bg-[linear-gradient(180deg,#22c55e,#38bdf8)] opacity-80 group-hover:opacity-100 transition-all duration-300" style={{ height: `${height}%` }} /></div>;
          })}
        </div>
      )}
    </section>
  );
}

function OrderTables({ openOrders, orderHistory, onCancel }: { openOrders: OrderDto[]; orderHistory: OrderDto[]; onCancel: (id: string) => void }) {
  return (
    <section className="grid gap-6 lg:grid-cols-2">
      <OrderTable title="Açık Emirler" orders={openOrders} onCancel={onCancel} cancellable />
      <OrderTable title="Emir Geçmişi" orders={orderHistory.slice(0, 8)} onCancel={onCancel} />
    </section>
  );
}

function OrderTable({ title, orders, onCancel, cancellable }: { title: string; orders: OrderDto[]; onCancel: (id: string) => void; cancellable?: boolean }) {
  return (
    <section className="rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-2xl flex flex-col">
      <h2 className="mb-6 text-xl font-black text-white">{title}</h2>
      {orders.length === 0 ? <div className="flex-1 flex"><State text="Kayıt bulunamadı." icon={Inbox} /></div> : <div className="overflow-x-auto rounded-xl border border-line"><table className="w-full text-left whitespace-nowrap"><thead className="text-xs sm:text-sm uppercase tracking-wider text-slate-500 bg-white/[0.02] border-b border-line"><tr><th className="p-4 font-bold">Yön</th><th className="p-4 font-bold">Sembol</th><th className="p-4 font-bold">Tip</th><th className="p-4 font-bold">Miktar</th><th className="p-4 font-bold">Fiyat</th><th className="p-4 font-bold">Durum</th>{cancellable && <th className="p-4" />}</tr></thead><tbody className="divide-y divide-line">{orders.map((o) => <tr key={o.id} className="transition-colors hover:bg-white/[0.02]"><td className={`p-4 font-black ${o.side === 'BUY' ? 'text-accent' : 'text-danger'}`}>{o.side === 'BUY' ? 'ALIŞ' : 'SATIŞ'}</td><td className="p-4 font-bold text-white">{o.symbol}</td><td className="p-4 text-slate-300">{o.type.replace('_', ' ')}</td><td className="p-4 text-slate-300">{o.quantity}</td><td className="p-4 font-bold text-white">{formatCurrency(o.averageFillPrice ?? o.price ?? o.triggerPrice ?? 0)}</td><td className="p-4"><StatusBadge status={o.status} /></td>{cancellable && <td className="p-4 text-right"><button onClick={() => onCancel(o.id)} className="min-h-[36px] rounded-lg border border-danger/30 bg-danger/10 px-4 text-sm font-bold text-rose-300 transition hover:bg-danger hover:text-white">İptal</button></td>}</tr>)}</tbody></table></div>}
    </section>
  );
}

function StatusBadge({ status }: { status: string }) {
  const color = status === 'FILLED' ? 'text-accent bg-accent/15 border-accent/20' : status === 'PENDING' ? 'text-cyan-300 bg-cyan-400/15 border-cyan-400/20' : status === 'CANCELLED' ? 'text-slate-300 bg-white/10 border-white/10' : 'text-danger bg-danger/15 border-danger/20';
  return <span className={`rounded-full border px-3 py-1.5 text-xs font-black tracking-wide ${color}`}>{status === 'FILLED' ? 'GERÇEKLEŞTİ' : status === 'PENDING' ? 'BEKLİYOR' : status === 'CANCELLED' ? 'İPTAL' : status}</span>;
}

function PortfolioAnalytics({ summary, assets, prices }: any) {
  const allocation = summary?.assetAllocation ?? assets.map((asset: any) => ({
    symbol: asset.symbol,
    value: asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost),
    allocationPercent: 0,
    unrealizedPnl: 0
  }));
  const maxValue = Math.max(...allocation.map((item: any) => Number(item.value) || 0), 1);

  return (
    <section className="rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-2xl">
      <div className="mb-6 flex items-center gap-3 border-b border-line pb-4">
        <div className="rounded-xl bg-cyan-500/10 p-2 text-cyan-400"><BarChart3 size={24} /></div>
        <h2 className="text-xl sm:text-2xl font-black text-white">Akıllı Varlık Yönetimi</h2>
      </div>
      <div className="grid gap-5 md:grid-cols-2">
        <div className="rounded-2xl bg-black/20 border border-line/50 p-5 sm:p-6">
          <div className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-5">Varlık Dağılımı</div>
          <div className="space-y-5">
            {allocation.length === 0 && <State text="Dağılım için varlık alın." icon={Wallet} />}
            {allocation.map((item: any) => (
              <div key={item.symbol} className="group">
                <div className="mb-2.5 flex justify-between text-base font-bold text-slate-200"><span>{coinShort(item.symbol)}</span><span className="text-white tracking-wide">{formatCurrency(item.value)}</span></div>
                <div className="h-3 sm:h-4 rounded-full bg-black border border-line overflow-hidden"><div className="h-full rounded-full bg-[linear-gradient(90deg,#22c55e,#38bdf8)] transition-all duration-1000 ease-out" style={{ width: `${Math.max(4, (Number(item.value) / maxValue) * 100)}%` }} /></div>
              </div>
            ))}
          </div>
        </div>
        <div className="rounded-2xl bg-black/20 border border-line/50 p-5 sm:p-6">
          <div className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-5">Kâr / Zarar Özeti</div>
          <div className="grid gap-4">
            <Metric title="Total PnL" value={formatCurrency(summary?.totalPnl ?? summary?.unrealizedPnl ?? 0)} sub="Gerçekleşmemiş PnL" />
            <Metric title="En İyi Performans" value={summary?.bestPerformer?.symbol ? coinShort(summary.bestPerformer.symbol) : '-'} sub={formatCurrency(summary?.bestPerformer?.unrealizedPnl ?? 0)} />
            <Metric title="En Zayıf Performans" value={summary?.worstPerformer?.symbol ? coinShort(summary.worstPerformer.symbol) : '-'} sub={formatCurrency(summary?.worstPerformer?.unrealizedPnl ?? 0)} />
          </div>
        </div>
      </div>
    </section>
  );
}

function AiInsight({ summary, assets }: any) {
  const totalAssetValue = Number(summary?.totalAssetValue ?? 0);
  const largest = (summary?.assetAllocation ?? []).slice().sort((a: any, b: any) => Number(b.value) - Number(a.value))[0];
  const concentration = totalAssetValue > 0 && largest ? Number(largest.value) / totalAssetValue * 100 : 0;
  const score = Math.max(32, Math.round(92 - concentration / 1.6));
  const message = concentration > 65
    ? `Portföyünüzün %${concentration.toFixed(0)} kadarı ${coinShort(largest.symbol)} üzerinde. Risk azaltmak için ETH, SOL ve LINK dağılımı düşünülebilir.`
    : 'Portföy dağılımı dengeli görünüyor. Volatilite için stop-loss ve kademeli alım kullanabilirsiniz.';

  return (
    <section className="rounded-2xl border border-cyan-400/30 bg-[linear-gradient(135deg,rgba(14,165,233,0.12),rgba(34,197,94,0.06))] p-5 sm:p-6 shadow-2xl shadow-cyan-500/5 relative overflow-hidden">
      <div className="absolute -right-10 -top-10 text-cyan-500/10 blur-2xl"><Sparkles size={160} /></div>
      <div className="mb-6 flex items-center gap-3 border-b border-cyan-500/20 pb-4 relative z-10">
        <div className="rounded-xl bg-cyan-500/20 p-2 text-cyan-300"><Sparkles size={24} /></div>
        <h2 className="text-xl sm:text-2xl font-black text-white drop-shadow-md">AI Insights</h2>
      </div>
      <div className="grid gap-4 sm:grid-cols-3 relative z-10">
        <Metric title="Risk Skoru" value={`${score}/100`} sub="Algoritmik Analiz" />
        <Metric title="Çeşitlilik" value={assets.length > 2 ? 'Güçlü' : 'Zayıf'} sub={`${assets.length} Aktif Varlık`} />
        <Metric title="Konsantrasyon" value={`${concentration.toFixed(0)}%`} sub={largest?.symbol ? coinShort(largest.symbol) : 'N/A'} />
      </div>
      <div className="mt-5 rounded-2xl border border-cyan-400/20 bg-black/30 p-5 text-base lg:text-lg leading-relaxed text-cyan-50 font-medium relative z-10 backdrop-blur-sm">{message}</div>
    </section>
  );
}

function RecentTrades({ transactions }: any) {
  const recent = transactions.slice(0, 6);
  return (
    <section className="rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-2xl flex flex-col">
      <h2 className="mb-6 text-xl font-black text-white">Son İşlemler</h2>
      {recent.length === 0 ? <div className="flex-1 flex"><State text="İşlem geçmişi boş." icon={History} /></div> : <div className="overflow-x-auto rounded-xl border border-line"><table className="w-full text-left text-base whitespace-nowrap"><thead className="text-xs sm:text-sm uppercase tracking-wider text-slate-500 bg-white/[0.02] border-b border-line"><tr><th className="p-4 font-bold">Tip</th><th className="p-4 font-bold">Sembol</th><th className="p-4 font-bold">Miktar</th><th className="p-4 font-bold">Fiyat</th><th className="p-4 font-bold">Durum</th></tr></thead><tbody className="divide-y divide-line">{recent.map((t: any) => <tr key={t.id} className="transition-colors hover:bg-white/[0.02]"><td className={`p-4 font-black ${t.type === 'BUY' ? 'text-accent' : 'text-danger'}`}>{t.type === 'BUY' ? 'ALIŞ' : 'SATIŞ'}</td><td className="p-4 font-bold text-white">{t.symbol}</td><td className="p-4 text-slate-300">{t.amount}</td><td className="p-4 font-bold text-white">{formatCurrency(t.price)}</td><td className="p-4"><span className="text-slate-400 font-medium">{t.status}</span></td></tr>)}</tbody></table></div>}
    </section>
  );
}

function WalletPage({ isLoading, isError, wallet, prices }: any) {
  if (isLoading) return <State text="Cüzdan verileri yükleniyor..." loading />;
  if (isError) return <State text="Cüzdan bilgisi alınamadı." icon={AlertCircle} />;
  if (!wallet) return <State text="Cüzdan bulunamadı." icon={Wallet} />;
  
  return (
    <div className="space-y-6 lg:space-y-8 animate-fadeIn">
      <div className="flex items-center gap-4 mb-2">
        <div className="p-3 bg-white/[0.04] rounded-2xl border border-line"><Wallet size={28} className="text-slate-200" /></div>
        <h1 className="text-3xl font-black text-white tracking-tight">Cüzdanım</h1>
      </div>
      
      <section className="grid gap-6 xl:grid-cols-[1fr_420px]">
        <div className="grid gap-6 sm:grid-cols-2 bg-panel/50 p-6 rounded-[2rem] border border-line">
          <Metric title="Toplam Portföy Değeri" value={formatCurrency(wallet.portfolioTotalValue)} sub="USDT + Kripto Varlıklar" />
          <Metric title="Kullanılabilir Bakiye" value={formatCurrency(wallet.availableBalance)} sub="İşlem İçin Uygun USDT" />
        </div>
        <div className="rounded-[2rem] border border-cyan-400/30 bg-[linear-gradient(135deg,rgba(14,165,233,0.3),rgba(34,197,94,0.15),rgba(15,23,42,0.95))] p-8 shadow-2xl transition-transform duration-500 hover:-translate-y-2 relative overflow-hidden">
          <div className="absolute -right-10 -top-10 text-white/5 blur-3xl"><CreditCard size={200} /></div>
          <div className="flex items-center justify-between relative z-10"><CreditCard size={32} className="text-white/80" /><ShieldCheck size={32} className="text-accent" /></div>
          <div className="mt-12 text-3xl font-black tracking-widest text-white drop-shadow-md relative z-10">{wallet.virtualCard?.maskedCardNumber ?? '**** **** **** ----'}</div>
          <div className="mt-8 flex justify-between text-lg font-bold text-cyan-50 relative z-10"><span>{wallet.virtualCard?.cardHolderName ?? 'Demo Kart'}</span><span className="tracking-widest">{String(wallet.virtualCard?.expiryMonth ?? 0).padStart(2, '0')}/{wallet.virtualCard?.expiryYear ?? '----'}</span></div>
        </div>
      </section>
      
      <AssetList assets={wallet.assets} prices={prices} />
    </div>
  );
}

function TransactionsPage({ isLoading, transactions }: any) {
  const [filter, setFilter] = useState<'ALL' | 'BUY' | 'SELL'>('ALL');
  const filtered = transactions.filter((t: any) => filter === 'ALL' || t.type === filter);
  
  if (isLoading) return <State text="İşlem geçmişi yükleniyor..." loading />;
  
  return (
    <section className="rounded-2xl border border-line bg-panel p-5 sm:p-8 shadow-2xl animate-fadeIn">
      <div className="mb-8 flex flex-col justify-between gap-5 sm:flex-row sm:items-center border-b border-line pb-6">
        <div className="flex items-center gap-4">
          <div className="p-3 bg-white/[0.04] rounded-2xl border border-line"><History size={28} className="text-slate-200" /></div>
          <h1 className="text-2xl sm:text-3xl font-black text-white tracking-tight">İşlem Geçmişi</h1>
        </div>
        <div className="grid grid-cols-3 rounded-xl bg-black/40 p-1.5 border border-line">
          {(['ALL', 'BUY', 'SELL'] as const).map((item) => (
            <button key={item} onClick={() => setFilter(item)} className={`min-h-[44px] rounded-lg px-4 text-sm font-black transition-colors ${filter === item ? 'bg-white/15 text-white shadow-md' : 'text-slate-400 hover:bg-white/5 hover:text-slate-200'}`}>
              {item === 'ALL' ? 'Tümü' : item === 'BUY' ? 'Alış' : 'Satış'}
            </button>
          ))}
        </div>
      </div>
      
      {filtered.length === 0 ? <State text="Geçmiş işlem bulunamadı." icon={History} /> : (
        <div className="overflow-x-auto rounded-xl border border-line">
          <table className="w-full text-left text-base whitespace-nowrap">
            <thead className="text-sm uppercase tracking-wider text-slate-500 bg-white/[0.02] border-b border-line">
              <tr><th className="p-5 font-bold">Tip</th><th className="p-5 font-bold">Sembol</th><th className="p-5 font-bold">Miktar</th><th className="p-5 font-bold">Fiyat</th><th className="p-5 font-bold">Toplam Değer</th><th className="p-5 font-bold">Tarih</th><th className="p-5 font-bold">Durum</th></tr>
            </thead>
            <tbody className="divide-y divide-line">
              {filtered.map((t: any) => (
                <tr key={t.id} className="transition-colors hover:bg-white/[0.02]">
                  <td className={`p-5 font-black ${t.type === 'BUY' ? 'text-accent' : 'text-danger'}`}>{t.type === 'BUY' ? 'ALIŞ' : 'SATIŞ'}</td>
                  <td className="p-5 font-bold text-white">{t.symbol}</td>
                  <td className="p-5 text-slate-300">{t.amount}</td>
                  <td className="p-5 font-bold text-white">{formatCurrency(t.price)}</td>
                  <td className="p-5 font-black text-white">{formatCurrency(t.total)}</td>
                  <td className="p-5 text-slate-400 font-medium">{new Date(t.createdAt).toLocaleString()}</td>
                  <td className="p-5"><StatusBadge status={t.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

function SettingsPage({ user, onSaved, setNotice }: { user?: UserDto; onSaved: (user: UserDto) => void; setNotice: (notice: { type: 'success' | 'error'; message: string }) => void }) {
  const [profile, setProfile] = useState(() => ({ fullName: user?.fullName ?? '', email: user?.email ?? '', userName: user?.userName ?? '', preferredCurrency: user?.preferredCurrency ?? 'USDT', themePreference: user?.themePreference ?? 'dark' }));
  const [passwords, setPasswords] = useState({ currentPassword: '', newPassword: '' });
  
  useEffect(() => { if (user) setProfile({ fullName: user.fullName, email: user.email, userName: user.userName, preferredCurrency: user.preferredCurrency, themePreference: user.themePreference }); }, [user]);
  
  const saveProfile = useMutation({ mutationFn: () => updateMe(profile), onSuccess: (saved) => { onSaved(saved); setNotice({ type: 'success', message: 'Ayarlar güncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Ayarlar kaydedilemedi.' }) });
  const savePassword = useMutation({ mutationFn: () => changePassword(passwords), onSuccess: () => { setPasswords({ currentPassword: '', newPassword: '' }); setNotice({ type: 'success', message: 'Şifre başarıyla güncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Şifre güncellenemedi.' }) });
  
  return (
    <div className="animate-fadeIn">
      <div className="flex items-center gap-4 mb-8">
        <div className="p-3 bg-white/[0.04] rounded-2xl border border-line"><Settings size={28} className="text-slate-200" /></div>
        <h1 className="text-3xl font-black text-white tracking-tight">Ayarlar</h1>
      </div>
      
      <section className="grid gap-6 xl:grid-cols-2">
        <div className="rounded-[2rem] border border-line bg-panel p-6 sm:p-8 shadow-2xl">
          <h2 className="mb-6 text-2xl font-black text-white border-b border-line pb-4">Profil Bilgileri</h2>
          <div className="space-y-5">
            <Input label="Ad Soyad" value={profile.fullName} onChange={(fullName) => setProfile({ ...profile, fullName })} />
            <Input label="Email Adresi" value={profile.email} onChange={(email) => setProfile({ ...profile, email })} type="email" />
            <Input label="Kullanıcı Adı" value={profile.userName} onChange={(userName) => setProfile({ ...profile, userName })} />
            <Input label="Para Birimi" value={profile.preferredCurrency} onChange={(preferredCurrency) => setProfile({ ...profile, preferredCurrency })} />
            <button disabled={saveProfile.isPending} onClick={() => saveProfile.mutate()} className="mt-4 flex w-full min-h-[56px] items-center justify-center rounded-2xl bg-accent px-6 py-4 text-lg font-black text-white shadow-xl shadow-accent/20 transition hover:bg-accent/90 disabled:opacity-70">
              {saveProfile.isPending ? <Loader2 className="animate-spin" size={24} /> : 'Profili Kaydet'}
            </button>
          </div>
        </div>
        
        <div className="rounded-[2rem] border border-line bg-panel p-6 sm:p-8 shadow-2xl h-fit">
          <h2 className="mb-6 text-2xl font-black text-white border-b border-line pb-4">Güvenlik</h2>
          <div className="space-y-5">
            <Input label="Mevcut Şifre" type="password" value={passwords.currentPassword} onChange={(currentPassword) => setPasswords({ ...passwords, currentPassword })} />
            <Input label="Yeni Şifre" type="password" value={passwords.newPassword} onChange={(newPassword) => setPasswords({ ...passwords, newPassword })} />
            <button disabled={savePassword.isPending} onClick={() => savePassword.mutate()} className="mt-4 flex w-full min-h-[56px] items-center justify-center rounded-2xl bg-cyan-600 px-6 py-4 text-lg font-black text-white shadow-xl shadow-cyan-600/20 transition hover:bg-cyan-500 disabled:opacity-70">
              {savePassword.isPending ? <Loader2 className="animate-spin" size={24} /> : 'Şifreyi Değiştir'}
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}

function AssetList({ assets, prices }: any) {
  return (
    <div className="rounded-2xl border border-line bg-panel p-5 sm:p-8 shadow-2xl">
      <h2 className="mb-6 text-xl sm:text-2xl font-black text-white border-b border-line pb-4">Varlıklarım</h2>
      <div className="space-y-4">
        {assets.length === 0 && <State text="Henüz varlık bulunmuyor." icon={Wallet} />}
        {assets.map((asset: any) => (
          <div key={asset.id} className="grid gap-4 sm:gap-6 rounded-2xl border border-line bg-black/20 p-5 sm:p-6 text-base sm:grid-cols-4 transition hover:bg-white/[0.02]">
            <div className="flex items-center gap-4">
              <div className="grid h-12 w-12 place-items-center rounded-xl bg-white/[0.04] text-lg font-black border border-line/50">{coinShort(asset.symbol).slice(0, 3)}</div>
              <div>
                <div className="text-xl font-black text-white tracking-tight">{coinShort(asset.symbol)}</div>
                <div className="text-sm font-medium text-slate-500">{asset.symbol}</div>
              </div>
            </div>
            <Info label="Miktar" value={asset.amount} />
            <Info label="Ortalama Maliyet" value={formatCurrency(asset.averageCost)} />
            <Info label="Güncel Değer" value={formatCurrency(asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost))} strong />
          </div>
        ))}
      </div>
    </div>
  );
}

function Brand() { 
  return (
    <div className="flex items-center gap-4">
      <div className="grid h-12 w-12 sm:h-14 sm:w-14 place-items-center rounded-2xl bg-gradient-to-br from-accent/20 to-cyan-500/20 text-accent border border-accent/20 shadow-lg shadow-accent/10">
        <Activity size={28} className="drop-shadow-md" />
      </div>
      <div>
        <div className="text-2xl sm:text-3xl font-black tracking-tight text-white drop-shadow-sm">TRade<span className="text-accent">Turk</span></div>
        <div className="text-sm font-medium text-slate-400 tracking-wide uppercase mt-0.5">Pro Terminal</div>
      </div>
    </div>
  ); 
}

function UserPanel({ user, onLogout }: { user?: UserDto; onLogout: () => void }) { 
  return (
    <div className="absolute bottom-8 left-6 right-6 rounded-2xl border border-line bg-black/40 p-5 backdrop-blur-md">
      <div className="mb-5 flex items-center gap-4">
        <div className="grid h-12 w-12 place-items-center rounded-xl bg-gradient-to-br from-slate-700 to-slate-800 text-lg font-black text-white shadow-inner border border-line/50">
          {user?.fullName?.slice(0, 2).toUpperCase() ?? 'TT'}
        </div>
        <div className="overflow-hidden">
          <div className="text-base font-black text-white truncate">{user?.fullName ?? 'TRadeTurk'}</div>
          <div className="text-sm font-medium text-slate-500 truncate">{user?.email}</div>
        </div>
      </div>
      <button onClick={onLogout} className="flex min-h-[44px] w-full items-center justify-center gap-2 rounded-xl border border-line bg-white/[0.02] font-bold text-slate-300 transition hover:bg-white/10 hover:text-white">
        <LogOut size={18} /> Çıkış Yap
      </button>
    </div>
  ); 
}

function Metric({ title, value, sub }: { title: string; value: string; sub: string }) { 
  return (
    <article className="rounded-2xl border border-line bg-panel p-5 sm:p-6 shadow-xl relative overflow-hidden group">
      <div className="absolute inset-0 bg-gradient-to-br from-white/[0.02] to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
      <div className="relative z-10">
        <div className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-3">{title}</div>
        <div className="text-2xl sm:text-3xl font-black text-white tracking-tight drop-shadow-sm">{value}</div>
        <div className="mt-3 text-sm font-bold text-cyan-400">{sub}</div>
      </div>
    </article>
  ); 
}

function MiniStat({ label, value }: { label: string; value: string }) { 
  return (
    <div className="flex flex-col justify-center">
      <div className="text-xs sm:text-sm font-bold text-slate-500 uppercase tracking-wider mb-1">{label}</div>
      <div className="text-sm sm:text-lg font-black text-white">{value}</div>
    </div>
  ); 
}

function Row({ label, value, strong }: { label: string; value: string; strong?: boolean }) { 
  return (
    <div className={`flex justify-between items-center ${strong ? 'border-t border-line/50 pt-3.5 mt-1 text-lg font-black text-white' : 'text-slate-300 font-medium'}`}>
      <span>{label}</span>
      <span className={strong ? 'text-xl drop-shadow-md' : 'text-white font-bold'}>{value}</span>
    </div>
  ); 
}

function Info({ label, value, strong }: { label: string; value: string | number; strong?: boolean }) { 
  return (
    <div className="flex flex-col justify-center">
      <div className="text-sm font-bold text-slate-500 uppercase tracking-wider mb-1.5">{label}</div>
      <div className={`text-xl ${strong ? 'font-black text-accent' : 'font-bold text-white'}`}>{value}</div>
    </div>
  ); 
}

function State({ text, icon: Icon, loading }: { text: string; icon?: any; loading?: boolean }) { 
  return (
    <div className="flex w-full flex-col items-center justify-center rounded-2xl border border-line bg-white/[0.01] p-10 text-center animate-fadeIn">
      {loading ? <Loader2 className="mb-4 animate-spin text-accent" size={36} /> : Icon ? <Icon className="mb-4 text-slate-600" size={36} strokeWidth={1.5} /> : null}
      <div className="text-base font-medium text-slate-400">{text}</div>
    </div>
  ); 
}

function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; onChange: (value: string) => void; type?: string }) { 
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-bold text-slate-400 uppercase tracking-wider">{label}</span>
      <input type={type} value={value} onChange={(event) => onChange(event.target.value)} className="min-h-[56px] w-full rounded-2xl border border-line bg-panelSoft px-5 py-4 text-lg font-medium text-white shadow-inner outline-none transition focus:border-cyan-500/50 focus:bg-white/[0.04]" />
    </label>
  ); 
}

function Toast({ type, message, onClose }: { type: 'success' | 'error'; message: string; onClose: () => void }) { 
  return (
    <div className={`fixed right-4 sm:right-6 top-6 sm:top-8 z-[100] flex w-[calc(100%-32px)] sm:w-auto max-w-md items-center gap-4 rounded-2xl border p-5 text-base shadow-2xl animate-slideUp ${type === 'success' ? 'border-accent/40 bg-accent/20 text-emerald-50 backdrop-blur-xl' : 'border-danger/40 bg-danger/20 text-rose-50 backdrop-blur-xl'}`}>
      <CheckCircle2 className={`flex-shrink-0 ${type === 'success' ? 'text-accent' : 'text-danger'}`} size={24} />
      <span className="font-medium flex-1">{message}</span>
      <button onClick={onClose} className="p-1 opacity-70 hover:opacity-100 transition-opacity"><X size={20} /></button>
    </div>
  ); 
}
