import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Activity,
  BarChart3,
  Bell,
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
  X
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
  'BTCUSDT',
  'ETHUSDT',
  'BNBUSDT',
  'SOLUSDT',
  'XRPUSDT',
  'ADAUSDT',
  'DOGEUSDT',
  'AVAXUSDT',
  'DOTUSDT',
  'LINKUSDT',
  'MATICUSDT',
  'LTCUSDT',
  'TRXUSDT',
  'ATOMUSDT',
  'NEARUSDT'
] as const;

const initialPrices: Record<string, PriceState> = Object.fromEntries(
  defaultSymbols.map((symbol) => [symbol, { symbol, price: 0, previousPrice: 0 }])
) as Record<string, PriceState>;

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: value > 100 ? 2 : 5 }).format(value || 0);
const formatNumber = (value: number) => new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value || 0);
const coinShort = (symbol: string) => symbol.replace('USDT', '');
const coinName = (symbol: string) => ({
  BTCUSDT: 'Bitcoin',
  ETHUSDT: 'Ethereum',
  BNBUSDT: 'BNB',
  SOLUSDT: 'Solana',
  XRPUSDT: 'XRP',
  ADAUSDT: 'Cardano',
  DOGEUSDT: 'Dogecoin',
  AVAXUSDT: 'Avalanche',
  DOTUSDT: 'Polkadot',
  LINKUSDT: 'Chainlink',
  MATICUSDT: 'Polygon',
  LTCUSDT: 'Litecoin',
  TRXUSDT: 'TRON',
  ATOMUSDT: 'Cosmos',
  NEARUSDT: 'NEAR'
}[symbol] ?? symbol);

export function App() {
  const queryClient = useQueryClient();
  const [token, setToken] = useState(() => tokenStore.get());
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [page, setPage] = useState<Page>('dashboard');
  const [mobileOpen, setMobileOpen] = useState(false);
  const [prices, setPrices] = useState(initialPrices);
  const [connectionLabel, setConnectionLabel] = useState('Baglaniyor...');
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
      .then(() => setConnectionLabel(connection.state === HubConnectionState.Connected ? 'Canli Veri Bagli' : 'Baglaniyor...'))
      .catch(() => setConnectionLabel('Baglanti Hatasi'));
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
    if (!numericAmount || numericAmount <= 0) return setNotice({ type: 'error', message: 'Lutfen gecerli bir miktar girin.' });
    if (!selectedPrice || selectedPrice <= 0) return setNotice({ type: 'error', message: 'Canli fiyat henuz hazir degil.' });
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
    [Wallet, 'Cuzdanim', 'wallet'],
    [History, 'Islem Gecmisi', 'transactions'],
    [Settings, 'Ayarlar', 'settings']
  ] as const;

  return (
    <div className="min-h-screen bg-ink text-base text-slate-100">
      {notice && <Toast type={notice.type} message={notice.message} onClose={() => setNotice(null)} />}
      <aside className="fixed inset-y-0 left-0 hidden w-72 border-r border-line bg-[#0b1020]/95 px-6 py-7 backdrop-blur-xl lg:block">
        <Brand />
        <nav className="mt-8 space-y-2">
          {nav.map(([Icon, label, target]) => (
            <button key={target} onClick={() => setPage(target)} className={`flex w-full items-center gap-3 rounded-2xl px-4 py-3 text-left text-base font-bold transition ${page === target ? 'bg-accent/15 text-accent shadow-xl shadow-accent/5' : 'text-slate-300 hover:bg-white/5'}`}>
              <Icon size={19} />
              <span>{label}</span>
            </button>
          ))}
        </nav>
        <UserPanel user={meQuery.data} onLogout={logout} />
      </aside>

      {mobileOpen && (
        <div className="fixed inset-0 z-30 bg-black/70 lg:hidden">
          <div className="h-full w-80 bg-[#0b1020] p-6">
            <button className="mb-6 grid h-11 w-11 place-items-center rounded-xl border border-line" onClick={() => setMobileOpen(false)}><X /></button>
            <Brand />
            <nav className="mt-8 space-y-3">
              {nav.map(([Icon, label, target]) => (
                <button key={target} onClick={() => { setPage(target); setMobileOpen(false); }} className="flex w-full items-center gap-3 rounded-xl px-3 py-3 text-left text-base font-bold text-slate-200">
                  <Icon size={20} /> {label}
                </button>
              ))}
            </nav>
          </div>
        </div>
      )}

      <main className="lg:pl-72">
        <header className="sticky top-0 z-20 border-b border-line bg-ink/85 px-5 py-4 backdrop-blur-xl lg:px-8">
          <div className="flex items-center justify-between gap-4">
            <button className="grid h-11 w-11 place-items-center rounded-xl border border-line bg-white/[0.03] lg:hidden" onClick={() => setMobileOpen(true)}><Menu size={19} /></button>
            <div className="flex min-w-0 flex-1 items-center gap-3 rounded-2xl border border-line bg-white/[0.04] px-5 py-3">
              <Search size={19} className="text-slate-500" />
              <input value={search} onChange={(event) => setSearch(event.target.value)} className="w-full bg-transparent text-base outline-none placeholder:text-slate-600" placeholder="Coin ara: BTC, SOL, XRP..." />
            </div>
            <div className="hidden items-center gap-2 rounded-2xl border border-line bg-white/[0.04] px-4 py-3 text-base font-semibold text-slate-300 sm:flex">
              <Wifi size={18} className="text-accent" /> {connectionLabel}
            </div>
            <button className="grid h-11 w-11 place-items-center rounded-xl border border-line bg-white/[0.04]"><Bell size={19} /></button>
          </div>
          <TickerStrip symbols={defaultSymbols} prices={prices} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} />
        </header>

        <div className="mx-auto max-w-[1500px] px-5 pb-28 pt-8 lg:px-8 lg:pb-8">
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
      <nav className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-4 border-t border-line bg-[#0b1020]/95 p-2 backdrop-blur-xl lg:hidden">
        {nav.map(([Icon, label, target]) => (
          <button key={target} onClick={() => setPage(target)} className={`flex min-h-11 flex-col items-center justify-center rounded-xl text-[11px] font-black ${page === target ? 'bg-accent/15 text-accent' : 'text-slate-400'}`}>
            <Icon size={18} />
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
    onError: (err) => setError(err instanceof Error ? err.message : 'Giris yapilamadi.')
  });

  return (
    <main className="grid min-h-screen place-items-center bg-ink px-5 text-slate-100">
      <section className="w-full max-w-xl rounded-2xl border border-line bg-panel/90 p-8 shadow-2xl shadow-cyan-500/10 backdrop-blur">
        <Brand />
        <h1 className="mt-8 text-3xl font-black text-white">{authMode === 'login' ? 'Hesabina giris yap' : 'Demo portfoyunu olustur'}</h1>
        <p className="mt-3 text-base text-slate-400">{authMode === 'login' ? 'Premium trading terminaline devam et.' : '50,000 USDT demo cuzdan ve sanal kart otomatik acilir.'}</p>
        {error && <div className="mt-5 rounded-2xl border border-danger/30 bg-danger/10 p-4 text-base text-rose-200">{error}</div>}
        <div className="mt-7 space-y-4">
          {authMode === 'register' && <Input label="Ad soyad" value={form.fullName} onChange={(fullName) => setForm({ ...form, fullName })} />}
          {authMode === 'register' && <Input label="Email" value={form.email} onChange={(email) => setForm({ ...form, email })} />}
          {authMode === 'register' && <Input label="Kullanici adi" value={form.userName} onChange={(userName) => setForm({ ...form, userName })} />}
          {authMode === 'login' && <Input label="Email veya kullanici adi" value={form.emailOrUserName} onChange={(emailOrUserName) => setForm({ ...form, emailOrUserName })} />}
          <Input label="Sifre" type="password" value={form.password} onChange={(password) => setForm({ ...form, password })} />
          <button disabled={mutation.isPending} onClick={() => mutation.mutate()} className="flex w-full items-center justify-center gap-2 rounded-2xl bg-accent px-5 py-4 text-base font-black text-white shadow-xl shadow-accent/20 disabled:opacity-60">
            {mutation.isPending && <Loader2 className="animate-spin" />} {authMode === 'login' ? 'Giris Yap' : 'Kayit Ol'}
          </button>
          <button className="w-full py-2 text-base font-semibold text-cyan-300" onClick={() => { setError(''); setAuthMode(authMode === 'login' ? 'register' : 'login'); }}>
            {authMode === 'login' ? 'Yeni hesap olustur' : 'Zaten hesabim var'}
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
    <div className="space-y-6">
      <section className="grid gap-6 xl:grid-cols-4">
        <Metric title="Toplam Portfoy" value={formatCurrency(portfolioValue)} sub="Canli demo degeri" />
        <Metric title="Kullanilabilir USDT" value={formatCurrency(walletBalance)} sub="Aninda islem bakiyesi" />
        <Metric title="Gunluk K/Z" value={formatCurrency(summary?.dailyPnl ?? 0)} sub="Rule-based demo analitik" />
        <Metric title="Haftalik K/Z" value={formatCurrency(summary?.weeklyPnl ?? 0)} sub="Portfoy trendi" />
      </section>

      <section className="grid gap-6 xl:grid-cols-[300px_1fr_360px]">
        <Watchlist symbols={symbols} prices={prices} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} favorites={favorites} toggleFavorite={toggleFavorite} isLoading={isMarketLoading} />
        <ChartPanel symbol={selectedSymbol} ticker={selectedTicker} />
        <TradePanel mode={mode} setMode={setMode} orderType={orderType} setOrderType={setOrderType} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} amount={amount} setAmount={setAmount} limitPrice={limitPrice} setLimitPrice={setLimitPrice} triggerPrice={triggerPrice} setTriggerPrice={setTriggerPrice} selectedPrice={selectedPrice} fee={fee} total={total} submitTrade={submitTrade} isTrading={isTrading} symbols={symbols} />
      </section>

      <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
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
    <div className="mt-4 flex gap-3 overflow-x-auto pb-1">
      {symbols.map((symbol: string) => {
        const ticker = prices[symbol] ?? {};
        const positive = (ticker.changePercent24h ?? 0) >= 0;
        return (
          <button key={symbol} onClick={() => setSelectedSymbol(symbol)} className={`min-w-[170px] rounded-2xl border px-4 py-3 text-left transition hover:-translate-y-0.5 ${selectedSymbol === symbol ? 'border-accent/60 bg-accent/10 shadow-lg shadow-accent/10' : 'border-line bg-white/[0.03]'}`}>
            <div className="flex items-center justify-between text-sm font-black">
              <span>{coinShort(symbol)}</span>
              <span className={positive ? 'text-accent' : 'text-danger'}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</span>
            </div>
            <div className="mt-1 text-lg font-black text-white">{formatCurrency(ticker.price ?? 0)}</div>
          </button>
        );
      })}
    </div>
  );
}

function Watchlist({ symbols, prices, selectedSymbol, setSelectedSymbol, favorites, toggleFavorite, isLoading }: any) {
  return (
    <section className="rounded-2xl border border-line bg-panel/95 p-5 shadow-2xl">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-xl font-black">Live Watchlist</h2>
        {isLoading && <Loader2 className="animate-spin text-accent" size={18} />}
      </div>
      <div className="space-y-2">
        {symbols.map((symbol: string) => {
          const ticker = prices[symbol] ?? {};
          const positive = (ticker.changePercent24h ?? 0) >= 0;
          const favorite = favorites.includes(symbol);
          return (
            <button key={symbol} onClick={() => setSelectedSymbol(symbol)} className={`group grid w-full grid-cols-[1fr_auto] gap-3 rounded-2xl border p-3 text-left transition hover:bg-white/[0.06] ${selectedSymbol === symbol ? 'border-accent/50 bg-accent/10' : 'border-transparent bg-white/[0.025]'}`}>
              <div>
                <div className="flex items-center gap-2">
                  <button type="button" onClick={(event) => { event.stopPropagation(); toggleFavorite(symbol); }} className={favorite ? 'text-amber-300' : 'text-slate-600 group-hover:text-slate-300'}><Star size={15} fill={favorite ? 'currentColor' : 'none'} /></button>
                  <span className="text-base font-black">{coinShort(symbol)}</span>
                </div>
                <div className="mt-1 text-sm text-slate-500">{coinName(symbol)}</div>
              </div>
              <div className="text-right">
                <div className="text-base font-black">{formatCurrency(ticker.price ?? 0)}</div>
                <div className={positive ? 'text-sm font-bold text-accent' : 'text-sm font-bold text-danger'}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</div>
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
    <section className="overflow-hidden rounded-2xl border border-line bg-panel shadow-2xl">
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-line p-6">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-3xl font-black text-white">{coinShort(symbol)} / USDT</h1>
            <span className={positive ? 'rounded-full bg-accent/10 px-3 py-1 text-base font-black text-accent' : 'rounded-full bg-danger/10 px-3 py-1 text-base font-black text-danger'}>{positive ? '+' : ''}{(ticker.changePercent24h ?? 0).toFixed(2)}%</span>
          </div>
          <div className="mt-2 text-4xl font-black text-white">{formatCurrency(ticker.price ?? 0)}</div>
        </div>
        <div className="grid grid-cols-3 gap-4 text-right">
          <MiniStat label="24h High" value={formatCurrency(ticker.high24h ?? 0)} />
          <MiniStat label="24h Low" value={formatCurrency(ticker.low24h ?? 0)} />
          <MiniStat label="Volume" value={formatNumber(ticker.volume24h ?? 0)} />
        </div>
      </div>
      <div className="h-[520px] bg-black">
        <iframe title="TradingView Chart" src={chartUrl} className="h-full w-full border-0" />
      </div>
      <div className="flex gap-2 border-t border-line p-4">
        {['1m', '5m', '15m', '1h', '4h', '1d'].map((item) => <span key={item} className="rounded-xl bg-white/[0.04] px-3 py-2 text-sm font-bold text-slate-300">{item}</span>)}
      </div>
    </section>
  );
}

function TradePanel({ mode, setMode, orderType, setOrderType, selectedSymbol, setSelectedSymbol, amount, setAmount, limitPrice, setLimitPrice, triggerPrice, setTriggerPrice, selectedPrice, fee, total, submitTrade, isTrading, symbols }: any) {
  return (
    <section className="rounded-2xl border border-line bg-panel/95 p-6 shadow-2xl max-xl:sticky max-xl:bottom-20 max-xl:z-10">
      <div className="mb-5 flex items-center justify-between">
        <h2 className="text-xl font-black">Order Ticket</h2>
        {isTrading && <Loader2 className="animate-spin text-accent" size={18} />}
      </div>
      <div className="mb-5 grid grid-cols-2 rounded-2xl bg-black/25 p-1.5">
        {(['buy', 'sell'] as TradeMode[]).map((item) => <button key={item} onClick={() => setMode(item)} className={`rounded-xl py-3 text-base font-black ${mode === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>{item === 'buy' ? 'Alis' : 'Satis'}</button>)}
      </div>
      <label className="mb-2 block text-base font-semibold text-slate-300">Market</label>
      <select value={selectedSymbol} onChange={(event) => setSelectedSymbol(event.target.value)} className="mb-5 w-full rounded-2xl border border-line bg-panelSoft px-4 py-4 text-base outline-none">
        {symbols.map((symbol: string) => <option key={symbol} value={symbol}>{coinName(symbol)} ({coinShort(symbol)})</option>)}
      </select>
      <label className="mb-2 block text-base font-semibold text-slate-300">Order Type</label>
      <select value={orderType} onChange={(event) => setOrderType(event.target.value)} className="mb-5 w-full rounded-2xl border border-line bg-panelSoft px-4 py-4 text-base outline-none">
        <option value="MARKET">Market</option>
        <option value="LIMIT">Limit</option>
        <option value="STOP_LOSS">Stop Loss</option>
        <option value="TAKE_PROFIT">Take Profit</option>
      </select>
      <label className="mb-2 block text-base font-semibold text-slate-300">Miktar</label>
      <div className="mb-5 flex items-center rounded-2xl border border-line bg-panelSoft px-4">
        <input value={amount} onChange={(event) => setAmount(event.target.value)} type="number" min="0" step="0.00000001" className="w-full bg-transparent py-4 text-base outline-none" placeholder="0.00" />
        <span className="text-sm font-black text-slate-400">{coinShort(selectedSymbol)}</span>
      </div>
      {orderType === 'LIMIT' && <Input label="Limit fiyat" type="number" value={limitPrice} onChange={setLimitPrice} />}
      {(orderType === 'STOP_LOSS' || orderType === 'TAKE_PROFIT') && <Input label="Trigger fiyat" type="number" value={triggerPrice} onChange={setTriggerPrice} />}
      <div className="mb-6 space-y-3 rounded-2xl bg-black/25 p-5 text-base">
        <Row label="Birim Fiyat" value={formatCurrency(selectedPrice)} />
        <Row label="Komisyon (0.1%)" value={formatCurrency(fee)} />
        <Row label="Tahmini Toplam" value={formatCurrency(total)} strong />
      </div>
      <button disabled={isTrading} onClick={submitTrade} className={`w-full rounded-2xl py-4 text-base font-black text-white shadow-xl disabled:opacity-60 ${mode === 'buy' ? 'bg-accent shadow-accent/20' : 'bg-danger shadow-danger/20'}`}>
        {orderType === 'MARKET' ? (mode === 'buy' ? 'Market Alimi Onayla' : 'Market Satisi Onayla') : 'Emri Deftere Gonder'}
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
    <section className="rounded-2xl border border-line bg-panel p-6 shadow-2xl">
      <div className="mb-5 flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h2 className="text-2xl font-black">Portfolio History</h2>
          <p className="mt-1 text-base text-slate-400">Worker snapshot equity curve ve PnL trendi.</p>
        </div>
        <div className="grid grid-cols-5 rounded-2xl bg-black/25 p-1">
          {['1D', '7D', '1M', '3M', '1Y'].map((item) => <button key={item} onClick={() => setRange(item)} className={`min-h-11 rounded-xl px-3 text-sm font-black ${range === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>{item}</button>)}
        </div>
      </div>
      {data.length === 0 ? <State text="Snapshot worker ilk veriyi olusturdugunda grafik burada gorunecek." /> : (
        <div className="flex h-64 items-end gap-2 rounded-2xl bg-black/25 p-4">
          {data.map((point: any) => {
            const height = 12 + ((Number(point.totalValue) - min) / span) * 88;
            return <div key={point.createdAt} title={`${new Date(point.createdAt).toLocaleString()} ${formatCurrency(point.totalValue)}`} className="flex min-w-4 flex-1 flex-col justify-end"><div className="rounded-t-lg bg-[linear-gradient(180deg,#22c55e,#38bdf8)] transition-all" style={{ height: `${height}%` }} /></div>;
          })}
        </div>
      )}
    </section>
  );
}

function OrderTables({ openOrders, orderHistory, onCancel }: { openOrders: OrderDto[]; orderHistory: OrderDto[]; onCancel: (id: string) => void }) {
  return (
    <section className="grid gap-6 xl:grid-cols-2">
      <OrderTable title="Open Orders" orders={openOrders} onCancel={onCancel} cancellable />
      <OrderTable title="Order History" orders={orderHistory.slice(0, 8)} onCancel={onCancel} />
    </section>
  );
}

function OrderTable({ title, orders, onCancel, cancellable }: { title: string; orders: OrderDto[]; onCancel: (id: string) => void; cancellable?: boolean }) {
  return (
    <section className="rounded-2xl border border-line bg-panel p-6 shadow-2xl">
      <h2 className="mb-5 text-xl font-black">{title}</h2>
      {orders.length === 0 ? <State text="Gosterilecek emir yok." /> : <div className="overflow-x-auto"><table className="w-full text-left text-sm sm:text-base"><thead className="text-xs uppercase text-slate-500"><tr><th className="p-3">Side</th><th className="p-3">Symbol</th><th className="p-3">Type</th><th className="p-3">Qty</th><th className="p-3">Price</th><th className="p-3">Status</th>{cancellable && <th className="p-3" />}</tr></thead><tbody>{orders.map((o) => <tr key={o.id} className="border-t border-line"><td className={`p-3 font-black ${o.side === 'BUY' ? 'text-accent' : 'text-danger'}`}>{o.side}</td><td className="p-3">{o.symbol}</td><td className="p-3">{o.type}</td><td className="p-3">{o.quantity}</td><td className="p-3">{formatCurrency(o.averageFillPrice ?? o.price ?? o.triggerPrice ?? 0)}</td><td className="p-3"><StatusBadge status={o.status} /></td>{cancellable && <td className="p-3 text-right"><button onClick={() => onCancel(o.id)} className="min-h-11 rounded-xl border border-danger/30 px-3 font-bold text-danger">Iptal</button></td>}</tr>)}</tbody></table></div>}
    </section>
  );
}

function StatusBadge({ status }: { status: string }) {
  const color = status === 'FILLED' ? 'text-accent bg-accent/10' : status === 'PENDING' ? 'text-cyan-300 bg-cyan-400/10' : status === 'CANCELLED' ? 'text-slate-300 bg-white/10' : 'text-danger bg-danger/10';
  return <span className={`rounded-full px-3 py-1 text-xs font-black ${color}`}>{status}</span>;
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
    <section className="rounded-2xl border border-line bg-panel p-6 shadow-2xl">
      <div className="mb-5 flex items-center gap-3">
        <BarChart3 className="text-cyan-300" />
        <h2 className="text-2xl font-black">Akilli Varlik Yonetimi</h2>
      </div>
      <div className="grid gap-5 md:grid-cols-2">
        <div className="rounded-2xl bg-black/25 p-5">
          <div className="text-base font-bold text-slate-400">Asset Distribution</div>
          <div className="mt-5 space-y-4">
            {allocation.length === 0 && <State text="Portfoy dagilimi icin once alim yapin." />}
            {allocation.map((item: any) => (
              <div key={item.symbol}>
                <div className="mb-2 flex justify-between text-base font-bold"><span>{coinShort(item.symbol)}</span><span>{formatCurrency(item.value)}</span></div>
                <div className="h-3 rounded-full bg-white/5"><div className="h-3 rounded-full bg-[linear-gradient(90deg,#22c55e,#38bdf8)]" style={{ width: `${Math.max(4, (Number(item.value) / maxValue) * 100)}%` }} /></div>
              </div>
            ))}
          </div>
        </div>
        <div className="rounded-2xl bg-black/25 p-5">
          <div className="text-base font-bold text-slate-400">PnL Snapshot</div>
          <div className="mt-5 grid gap-4">
            <Metric title="Total PnL" value={formatCurrency(summary?.totalPnl ?? summary?.unrealizedPnl ?? 0)} sub="Unrealized + demo analytics" />
            <Metric title="Best Performer" value={summary?.bestPerformer?.symbol ? coinShort(summary.bestPerformer.symbol) : '-'} sub={formatCurrency(summary?.bestPerformer?.unrealizedPnl ?? 0)} />
            <Metric title="Worst Performer" value={summary?.worstPerformer?.symbol ? coinShort(summary.worstPerformer.symbol) : '-'} sub={formatCurrency(summary?.worstPerformer?.unrealizedPnl ?? 0)} />
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
    ? `Portfoyunuzun %${concentration.toFixed(0)} kadari ${coinShort(largest.symbol)} uzerinde. Risk azaltmak icin ETH, SOL ve LINK dagilimi dusunulebilir.`
    : 'Portfoy dagilimi dengeli gorunuyor. Volatilite icin stop-loss ve kademeli alim kullanabilirsiniz.';

  return (
    <section className="rounded-2xl border border-cyan-400/25 bg-[linear-gradient(135deg,rgba(14,165,233,0.14),rgba(34,197,94,0.08))] p-6 shadow-2xl">
      <div className="mb-5 flex items-center gap-3">
        <Sparkles className="text-cyan-300" />
        <h2 className="text-2xl font-black">TradeTurk AI Insights</h2>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <Metric title="Risk Score" value={`${score}/100`} sub="Rule-based v1" />
        <Metric title="Diversification" value={assets.length > 2 ? 'Strong' : 'Low'} sub={`${assets.length} aktif asset`} />
        <Metric title="Concentration" value={`${concentration.toFixed(0)}%`} sub={largest?.symbol ? coinShort(largest.symbol) : 'N/A'} />
      </div>
      <div className="mt-5 rounded-2xl border border-white/10 bg-black/25 p-5 text-base leading-7 text-slate-200">{message}</div>
    </section>
  );
}

function RecentTrades({ transactions }: any) {
  const recent = transactions.slice(0, 6);
  return (
    <section className="rounded-2xl border border-line bg-panel p-6 shadow-2xl">
      <h2 className="mb-5 text-xl font-black">Recent Trades</h2>
      {recent.length === 0 ? <State text="Henuz islem yok." /> : <div className="overflow-x-auto"><table className="w-full text-left text-base"><thead className="text-sm uppercase text-slate-500"><tr><th className="p-3">Tip</th><th className="p-3">Symbol</th><th className="p-3">Miktar</th><th className="p-3">Fiyat</th><th className="p-3">Durum</th></tr></thead><tbody>{recent.map((t: any) => <tr key={t.id} className="border-t border-line"><td className={`p-3 font-black ${t.type === 'BUY' ? 'text-accent' : 'text-danger'}`}>{t.type}</td><td className="p-3">{t.symbol}</td><td className="p-3">{t.amount}</td><td className="p-3">{formatCurrency(t.price)}</td><td className="p-3">{t.status}</td></tr>)}</tbody></table></div>}
    </section>
  );
}

function WalletPage({ isLoading, isError, wallet, prices }: any) {
  if (isLoading) return <State text="Cuzdan yukleniyor..." />;
  if (isError) return <State text="Cuzdan bilgisi alinamadi." />;
  if (!wallet) return <State text="Cuzdan bulunamadi." />;
  return <div className="space-y-6">
    <section className="grid gap-6 lg:grid-cols-[1fr_420px]">
      <Metric title="Toplam Portfoy Degeri" value={formatCurrency(wallet.portfolioTotalValue)} sub="USDT + asset degeri" />
      <div className="rounded-2xl border border-cyan-400/25 bg-[linear-gradient(135deg,rgba(14,165,233,0.26),rgba(34,197,94,0.16),rgba(15,23,42,0.9))] p-6 shadow-2xl transition hover:-translate-y-1">
        <div className="flex items-center justify-between"><CreditCard /><ShieldCheck className="text-accent" /></div>
        <div className="mt-10 text-2xl font-black tracking-wide">{wallet.virtualCard?.maskedCardNumber ?? '**** **** **** ----'}</div>
        <div className="mt-6 flex justify-between text-base font-bold text-slate-200"><span>{wallet.virtualCard?.cardHolderName ?? 'Demo Kart'}</span><span>{String(wallet.virtualCard?.expiryMonth ?? 0).padStart(2, '0')}/{wallet.virtualCard?.expiryYear ?? '----'}</span></div>
        <button className="mt-6 rounded-xl bg-white/10 px-4 py-2 text-sm font-bold text-slate-200">Kart Numarasini Kopyala</button>
      </div>
    </section>
    <section className="grid gap-6 md:grid-cols-2"><Metric title="Toplam Bakiye" value={formatCurrency(wallet.totalBalance)} sub="Demo sanal bakiye" /><Metric title="Kullanilabilir Bakiye" value={formatCurrency(wallet.availableBalance)} sub="Trade icin uygun" /></section>
    <AssetList assets={wallet.assets} prices={prices} />
  </div>;
}

function TransactionsPage({ isLoading, transactions }: any) {
  const [filter, setFilter] = useState<'ALL' | 'BUY' | 'SELL'>('ALL');
  const filtered = transactions.filter((t: any) => filter === 'ALL' || t.type === filter);
  if (isLoading) return <State text="Islem gecmisi yukleniyor..." />;
  return <section className="rounded-2xl border border-line bg-panel p-6 shadow-xl">
    <div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-center"><h1 className="text-2xl font-black">Islem Gecmisi</h1><div className="grid grid-cols-3 rounded-2xl bg-black/25 p-1.5">{(['ALL', 'BUY', 'SELL'] as const).map((item) => <button key={item} onClick={() => setFilter(item)} className={`rounded-xl px-4 py-2 text-base font-black ${filter === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>{item === 'ALL' ? 'Tumu' : item === 'BUY' ? 'Alis' : 'Satis'}</button>)}</div></div>
    {filtered.length === 0 ? <State text="Henuz islem yok." /> : <div className="overflow-x-auto"><table className="w-full text-left text-base"><thead className="text-sm uppercase text-slate-500"><tr><th className="p-4">Tip</th><th className="p-4">Symbol</th><th className="p-4">Miktar</th><th className="p-4">Fiyat</th><th className="p-4">Toplam</th><th className="p-4">Tarih</th><th className="p-4">Durum</th></tr></thead><tbody>{filtered.map((t: any) => <tr key={t.id} className="border-t border-line"><td className={`p-4 font-black ${t.type === 'BUY' ? 'text-accent' : 'text-danger'}`}>{t.type}</td><td className="p-4">{t.symbol}</td><td className="p-4">{t.amount}</td><td className="p-4">{formatCurrency(t.price)}</td><td className="p-4">{formatCurrency(t.total)}</td><td className="p-4 text-slate-300">{new Date(t.createdAt).toLocaleString()}</td><td className="p-4">{t.status}</td></tr>)}</tbody></table></div>}
  </section>;
}

function SettingsPage({ user, onSaved, setNotice }: { user?: UserDto; onSaved: (user: UserDto) => void; setNotice: (notice: { type: 'success' | 'error'; message: string }) => void }) {
  const [profile, setProfile] = useState(() => ({ fullName: user?.fullName ?? '', email: user?.email ?? '', userName: user?.userName ?? '', preferredCurrency: user?.preferredCurrency ?? 'USDT', themePreference: user?.themePreference ?? 'dark' }));
  const [passwords, setPasswords] = useState({ currentPassword: '', newPassword: '' });
  useEffect(() => { if (user) setProfile({ fullName: user.fullName, email: user.email, userName: user.userName, preferredCurrency: user.preferredCurrency, themePreference: user.themePreference }); }, [user]);
  const saveProfile = useMutation({ mutationFn: () => updateMe(profile), onSuccess: (saved) => { onSaved(saved); setNotice({ type: 'success', message: 'Ayarlar guncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Ayarlar kaydedilemedi.' }) });
  const savePassword = useMutation({ mutationFn: () => changePassword(passwords), onSuccess: () => { setPasswords({ currentPassword: '', newPassword: '' }); setNotice({ type: 'success', message: 'Sifre guncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Sifre guncellenemedi.' }) });
  return <section className="grid gap-6 xl:grid-cols-2"><div className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><h1 className="mb-6 text-3xl font-black">Ayarlar</h1><div className="space-y-4"><Input label="Ad soyad" value={profile.fullName} onChange={(fullName) => setProfile({ ...profile, fullName })} /><Input label="Email" value={profile.email} onChange={(email) => setProfile({ ...profile, email })} /><Input label="Kullanici adi" value={profile.userName} onChange={(userName) => setProfile({ ...profile, userName })} /><Input label="Para birimi" value={profile.preferredCurrency} onChange={(preferredCurrency) => setProfile({ ...profile, preferredCurrency })} /><Input label="Tema" value={profile.themePreference} onChange={(themePreference) => setProfile({ ...profile, themePreference })} /><button onClick={() => saveProfile.mutate()} className="rounded-2xl bg-accent px-6 py-4 text-lg font-black text-white">Kaydet</button></div></div><div className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><h2 className="mb-6 text-2xl font-black">Sifre Degistir</h2><div className="space-y-4"><Input label="Mevcut sifre" type="password" value={passwords.currentPassword} onChange={(currentPassword) => setPasswords({ ...passwords, currentPassword })} /><Input label="Yeni sifre" type="password" value={passwords.newPassword} onChange={(newPassword) => setPasswords({ ...passwords, newPassword })} /><button onClick={() => savePassword.mutate()} className="rounded-2xl bg-cyan-500 px-6 py-4 text-lg font-black text-white">Sifreyi Guncelle</button></div></div></section>;
}

function AssetList({ assets, prices }: any) {
  return <div className="rounded-2xl border border-line bg-panel p-6 shadow-2xl"><h2 className="mb-5 text-xl font-black">Varliklarim</h2><div className="space-y-4">{assets.length === 0 && <State text="Henuz varlik yok." />}{assets.map((asset: any) => <div key={asset.id} className="grid gap-4 rounded-2xl border border-line bg-white/[0.04] p-5 text-base sm:grid-cols-4"><div><div className="text-xl font-black">{coinShort(asset.symbol)}</div><div className="text-sm text-slate-500">{asset.symbol}</div></div><Info label="Miktar" value={asset.amount} /><Info label="Ortalama" value={formatCurrency(asset.averageCost)} /><Info label="Deger" value={formatCurrency(asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost))} /></div>)}</div></div>;
}

function Brand() { return <div className="flex items-center gap-3"><div className="grid h-12 w-12 place-items-center rounded-2xl bg-accent/15 text-accent"><Activity size={26} /></div><div><div className="text-2xl font-black tracking-wide">TRade<span className="text-accent">Turk</span></div><div className="text-sm text-slate-500">Pro Trading Desk</div></div></div>; }
function UserPanel({ user, onLogout }: { user?: UserDto; onLogout: () => void }) { return <div className="absolute bottom-7 left-6 right-6 rounded-2xl border border-line bg-white/[0.04] p-5"><div className="mb-4 flex items-center gap-3"><div className="grid h-12 w-12 place-items-center rounded-2xl bg-slate-700 text-lg font-black">{user?.fullName?.slice(0, 2).toUpperCase() ?? 'TT'}</div><div><div className="text-base font-black">{user?.fullName ?? 'TRadeTurk'}</div><div className="text-sm text-slate-500">{user?.email}</div></div></div><button onClick={onLogout} className="flex w-full items-center justify-center gap-2 rounded-xl border border-line py-3 font-bold text-slate-300"><LogOut size={18} /> Cikis</button></div>; }
function Metric({ title, value, sub }: { title: string; value: string; sub: string }) { return <article className="rounded-2xl border border-line bg-panel p-6 shadow-2xl"><div className="text-base font-semibold text-slate-400">{title}</div><div className="mt-3 text-2xl font-black text-white">{value}</div><div className="mt-3 text-base text-cyan-300">{sub}</div></article>; }
function MiniStat({ label, value }: { label: string; value: string }) { return <div><div className="text-sm text-slate-500">{label}</div><div className="text-base font-black text-white">{value}</div></div>; }
function Row({ label, value, strong }: { label: string; value: string; strong?: boolean }) { return <div className={`flex justify-between ${strong ? 'border-t border-line pt-3 text-lg font-black text-white' : 'text-slate-400'}`}><span>{label}</span><span>{value}</span></div>; }
function Info({ label, value }: { label: string; value: string | number }) { return <div><div className="text-sm text-slate-500">{label}</div><div className="text-lg font-bold">{value}</div></div>; }
function State({ text }: { text: string }) { return <div className="rounded-2xl border border-line bg-white/[0.03] p-6 text-lg text-slate-400">{text}</div>; }
function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; onChange: (value: string) => void; type?: string }) { return <label className="block"><span className="mb-2 block text-base font-semibold text-slate-300">{label}</span><input type={type} value={value} onChange={(event) => onChange(event.target.value)} className="w-full rounded-2xl border border-line bg-panelSoft px-4 py-4 text-lg outline-none focus:border-accent/60" /></label>; }
function Toast({ type, message, onClose }: { type: 'success' | 'error'; message: string; onClose: () => void }) { return <div className={`fixed right-5 top-5 z-50 flex max-w-md items-center gap-3 rounded-2xl border p-4 text-base shadow-xl ${type === 'success' ? 'border-accent/30 bg-accent/15 text-emerald-100' : 'border-danger/30 bg-danger/15 text-rose-100'}`}><CheckCircle2 className={type === 'success' ? 'text-accent' : 'text-danger'} /><span>{message}</span><button onClick={onClose}><X size={18} /></button></div>; }
