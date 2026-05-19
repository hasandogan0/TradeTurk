import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Activity, Bell, CheckCircle2, CreditCard, History, LayoutDashboard, Loader2, LogOut, Menu, Search, Settings, ShieldCheck, Wallet, Wifi, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { API_BASE_URL } from './config';
import { changePassword, executeTrade, getMe, getPortfolioSummary, getTransactions, getWallet, login, register, tokenStore, updateMe } from './api';
import type { Page, PriceState, TradeMode, UserDto } from './types';

const symbols = ['BTCUSDT', 'ETHUSDT'] as const;
const initialPrices: Record<string, PriceState> = {
  BTCUSDT: { symbol: 'BTCUSDT', price: 0, previousPrice: 0 },
  ETHUSDT: { symbol: 'ETHUSDT', price: 0, previousPrice: 0 }
};

const formatCurrency = (value: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value || 0);
const coinName = (symbol: string) => (symbol === 'BTCUSDT' ? 'Bitcoin' : 'Ethereum');
const coinShort = (symbol: string) => symbol.replace('USDT', '');

export function App() {
  const queryClient = useQueryClient();
  const [token, setToken] = useState(() => tokenStore.get());
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [page, setPage] = useState<Page>('dashboard');
  const [mobileOpen, setMobileOpen] = useState(false);
  const [prices, setPrices] = useState(initialPrices);
  const [connectionLabel, setConnectionLabel] = useState('Baglaniyor...');
  const [mode, setMode] = useState<TradeMode>('buy');
  const [selectedSymbol, setSelectedSymbol] = useState('BTCUSDT');
  const [amount, setAmount] = useState('');
  const [notice, setNotice] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  const meQuery = useQuery({ queryKey: ['me'], queryFn: getMe, enabled: Boolean(token), retry: false });
  const walletQuery = useQuery({ queryKey: ['wallet'], queryFn: getWallet, enabled: Boolean(token) });
  const transactionsQuery = useQuery({ queryKey: ['transactions'], queryFn: getTransactions, enabled: Boolean(token) });
  const portfolioQuery = useQuery({ queryKey: ['portfolio-summary'], queryFn: getPortfolioSummary, enabled: Boolean(token) });

  useEffect(() => {
    if (meQuery.isError) {
      tokenStore.clear();
      setToken(null);
    }
  }, [meQuery.isError]);

  useEffect(() => {
    const connection = new HubConnectionBuilder().withUrl(`${API_BASE_URL}/priceHub`).withAutomaticReconnect().build();
    connection.on('ReceivePriceUpdate', (symbol: string, price: number) => {
      setPrices((current) => ({
        ...current,
        [symbol]: { symbol, previousPrice: current[symbol]?.price ?? 0, price: Number(price) }
      }));
    });
    connection.start()
      .then(() => setConnectionLabel(connection.state === HubConnectionState.Connected ? 'Canli Veri Bagli' : 'Baglaniyor...'))
      .catch(() => setConnectionLabel('Baglanti Hatasi'));
    return () => { void connection.stop(); };
  }, []);

  const selectedPrice = prices[selectedSymbol]?.price ?? 0;
  const numericAmount = Number(amount) || 0;
  const fee = numericAmount * selectedPrice * 0.001;
  const gross = numericAmount * selectedPrice;
  const total = mode === 'buy' ? gross + fee : gross - fee;

  const tradeMutation = useMutation({
    mutationFn: () => executeTrade(mode, { symbol: selectedSymbol, amount: numericAmount, requestedPrice: selectedPrice }),
    onSuccess: async (result) => {
      setNotice({ type: result.isSuccess ? 'success' : 'error', message: result.message });
      setAmount('');
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['wallet'] }),
        queryClient.invalidateQueries({ queryKey: ['transactions'] }),
        queryClient.invalidateQueries({ queryKey: ['portfolio-summary'] })
      ]);
    },
    onError: (error) => setNotice({ type: 'error', message: error instanceof Error ? error.message : 'Islem basarisiz oldu.' })
  });

  const portfolioValue = portfolioQuery.data?.totalPortfolioValue ?? walletQuery.data?.portfolioTotalValue ?? 0;
  const assets = walletQuery.data?.assets ?? [];

  function logout() {
    tokenStore.clear();
    setToken(null);
    queryClient.clear();
  }

  function submitTrade() {
    setNotice(null);
    if (!numericAmount || numericAmount <= 0) return setNotice({ type: 'error', message: 'Lutfen gecerli bir miktar girin.' });
    if (!selectedPrice || selectedPrice <= 0) return setNotice({ type: 'error', message: 'Canli fiyat henuz hazir degil.' });
    tradeMutation.mutate();
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
      <aside className="fixed inset-y-0 left-0 hidden w-80 border-r border-line bg-[#0b1020]/95 px-7 py-8 backdrop-blur-xl lg:block">
        <Brand />
        <nav className="mt-10 space-y-3">
          {nav.map(([Icon, label, target]) => (
            <button key={target} onClick={() => setPage(target)} className={`flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left text-base font-bold transition ${page === target ? 'bg-accent/15 text-accent shadow-xl shadow-accent/5' : 'text-slate-300 hover:bg-white/5'}`}>
              <Icon size={22} />
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
                <button key={target} onClick={() => { setPage(target); setMobileOpen(false); }} className="flex w-full items-center gap-4 rounded-2xl px-4 py-4 text-left text-base font-bold text-slate-200">
                  <Icon size={22} /> {label}
                </button>
              ))}
            </nav>
          </div>
        </div>
      )}

      <main className="lg:pl-80">
        <header className="sticky top-0 z-20 border-b border-line bg-ink/85 px-5 py-4 backdrop-blur-xl lg:px-8">
          <div className="flex items-center justify-between gap-4">
            <button className="grid h-12 w-12 place-items-center rounded-xl border border-line bg-white/[0.03] lg:hidden" onClick={() => setMobileOpen(true)}><Menu /></button>
            <div className="flex min-w-0 flex-1 items-center gap-3 rounded-2xl border border-line bg-white/[0.04] px-5 py-4">
              <Search size={20} className="text-slate-500" />
              <input className="w-full bg-transparent text-base outline-none placeholder:text-slate-600" placeholder="Varlik ara (BTC, ETH, SOL...)" />
            </div>
            <div className="hidden items-center gap-2 rounded-2xl border border-line bg-white/[0.04] px-4 py-3 text-base font-semibold text-slate-300 sm:flex">
              <Wifi size={18} className="text-accent" /> {connectionLabel}
            </div>
            <button className="grid h-12 w-12 place-items-center rounded-xl border border-line bg-white/[0.04]"><Bell size={20} /></button>
          </div>
        </header>

        <div className="mx-auto max-w-7xl px-5 py-8 lg:px-8">
          {page === 'dashboard' && <Dashboard prices={prices} summary={portfolioQuery.data} walletBalance={walletQuery.data?.availableBalance ?? 0} portfolioValue={portfolioValue} mode={mode} setMode={setMode} selectedSymbol={selectedSymbol} setSelectedSymbol={setSelectedSymbol} amount={amount} setAmount={setAmount} selectedPrice={selectedPrice} fee={fee} total={total} submitTrade={submitTrade} isTrading={tradeMutation.isPending} assets={assets} />}
          {page === 'wallet' && <WalletPage isLoading={walletQuery.isLoading} isError={walletQuery.isError} wallet={walletQuery.data} prices={prices} />}
          {page === 'transactions' && <TransactionsPage isLoading={transactionsQuery.isLoading} transactions={transactionsQuery.data ?? []} />}
          {page === 'settings' && <SettingsPage user={meQuery.data} onSaved={(user) => queryClient.setQueryData(['me'], user)} setNotice={setNotice} />}
        </div>
      </main>
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
      <section className="w-full max-w-xl rounded-2xl border border-line bg-panel/90 p-8 shadow-xl shadow-cyan-500/10 backdrop-blur">
        <Brand />
        <h1 className="mt-8 text-3xl font-black text-white">{authMode === 'login' ? 'Hesabina giris yap' : 'Demo portfoyunu olustur'}</h1>
        <p className="mt-3 text-base text-slate-400">{authMode === 'login' ? 'Token ile korunan TRadeTurk paneline devam et.' : 'Kayitta 50,000 USDT demo cüzdan ve sanal kart otomatik acilir.'}</p>
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
  const { prices, summary, walletBalance, portfolioValue, mode, setMode, selectedSymbol, setSelectedSymbol, amount, setAmount, selectedPrice, fee, total, submitTrade, isTrading, assets } = props;
  return (
    <>
      <section className="mb-8 grid gap-6 xl:grid-cols-3">
        <Metric title="Toplam Portfoy" value={formatCurrency(portfolioValue)} sub="Canli demo degeri" />
        <Metric title="Kullanilabilir USDT" value={formatCurrency(walletBalance)} sub="Alis islemlerinde kullanilir" />
        <Metric title="Gerceklesmemis K/Z" value={formatCurrency(summary?.unrealizedPnl ?? 0)} sub="Asset maliyetine gore" />
      </section>
      <section className="mb-8 grid gap-6 md:grid-cols-2">
        {symbols.map((symbol) => {
          const price = prices[symbol];
          const direction = price.price >= price.previousPrice ? 'text-accent' : 'text-danger';
          return <article key={symbol} className="rounded-2xl border border-line bg-panel p-6 shadow-xl">
            <div className="flex items-start justify-between gap-4">
              <div className="flex items-center gap-4"><div className="grid h-14 w-14 place-items-center rounded-2xl bg-white/5 text-lg font-black">{coinShort(symbol)[0]}</div><div><div className="text-xl font-black">{coinShort(symbol)}</div><div className="text-sm text-slate-400">{coinName(symbol)}</div></div></div>
              <div className={`text-right text-xl font-black ${direction}`}>{formatCurrency(price.price)}</div>
            </div>
            <div className="mt-6 h-20 rounded-2xl bg-[linear-gradient(135deg,rgba(34,197,94,0.24),rgba(56,189,248,0.12))]" />
          </article>;
        })}
      </section>
      <section className="grid gap-6 xl:grid-cols-[440px_1fr]">
        <div className="rounded-2xl border border-line bg-panel p-6 shadow-xl">
          <div className="mb-5 flex items-center justify-between"><h2 className="text-xl font-black">Hizli Islem</h2>{isTrading && <Loader2 className="animate-spin text-accent" />}</div>
          <div className="mb-5 grid grid-cols-2 rounded-2xl bg-black/25 p-1.5">{(['buy', 'sell'] as TradeMode[]).map((item) => <button key={item} onClick={() => setMode(item)} className={`rounded-xl py-3 text-base font-black ${mode === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>{item === 'buy' ? 'Alis' : 'Satis'}</button>)}</div>
          <label className="mb-2 block text-base font-semibold text-slate-300">Varlik</label>
          <select value={selectedSymbol} onChange={(event) => setSelectedSymbol(event.target.value)} className="mb-5 w-full rounded-2xl border border-line bg-panelSoft px-4 py-4 text-base outline-none">{symbols.map((symbol) => <option key={symbol} value={symbol}>{coinName(symbol)} ({coinShort(symbol)})</option>)}</select>
          <label className="mb-2 block text-base font-semibold text-slate-300">Miktar</label>
          <div className="mb-5 flex items-center rounded-2xl border border-line bg-panelSoft px-4"><input value={amount} onChange={(event) => setAmount(event.target.value)} type="number" min="0" step="0.00000001" className="w-full bg-transparent py-4 text-base outline-none" placeholder="0.00" /><span className="text-sm font-black text-slate-400">{coinShort(selectedSymbol)}</span></div>
          <div className="mb-6 space-y-3 rounded-2xl bg-black/25 p-5 text-base"><Row label="Birim Fiyat" value={formatCurrency(selectedPrice)} /><Row label="Komisyon (0.1%)" value={formatCurrency(fee)} /><Row label="Toplam" value={formatCurrency(total)} strong /></div>
          <button disabled={isTrading} onClick={submitTrade} className={`w-full rounded-2xl py-4 text-base font-black text-white shadow-xl disabled:opacity-60 ${mode === 'buy' ? 'bg-accent shadow-accent/20' : 'bg-danger shadow-danger/20'}`}>{mode === 'buy' ? 'Satin Alimi Onayla' : 'Satisi Onayla'}</button>
        </div>
        <AssetList assets={assets} prices={prices} />
      </section>
    </>
  );
}

function WalletPage({ isLoading, isError, wallet, prices }: any) {
  if (isLoading) return <State text="Cuzdan yukleniyor..." />;
  if (isError) return <State text="Cuzdan bilgisi alinamadi." />;
  if (!wallet) return <State text="Cuzdan bulunamadi." />;
  return <div className="space-y-6">
    <section className="grid gap-6 lg:grid-cols-[1fr_420px]">
      <Metric title="Toplam Portfoy Degeri" value={formatCurrency(wallet.portfolioTotalValue)} sub="USDT + asset degeri" />
      <div className="rounded-2xl border border-cyan-400/25 bg-[linear-gradient(135deg,rgba(14,165,233,0.22),rgba(34,197,94,0.16))] p-6 shadow-xl">
        <div className="flex items-center justify-between"><CreditCard /><ShieldCheck className="text-accent" /></div>
        <div className="mt-8 text-2xl font-black tracking-wide">{wallet.virtualCard?.maskedCardNumber ?? '**** **** **** ----'}</div>
        <div className="mt-5 flex justify-between text-base font-bold text-slate-200"><span>{wallet.virtualCard?.cardHolderName ?? 'Demo Kart'}</span><span>{String(wallet.virtualCard?.expiryMonth ?? 0).padStart(2, '0')}/{wallet.virtualCard?.expiryYear ?? '----'}</span></div>
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
    <div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-center"><h1 className="text-2xl font-black">Islem Gecmisi</h1><div className="grid grid-cols-3 rounded-2xl bg-black/25 p-1.5">{(['ALL', 'BUY', 'SELL'] as const).map((item) => <button key={item} onClick={() => setFilter(item)} className={`rounded-xl px-4 py-2 text-sm font-black ${filter === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>{item === 'ALL' ? 'Tumu' : item === 'BUY' ? 'Alis' : 'Satis'}</button>)}</div></div>
    {filtered.length === 0 ? <State text="Henuz islem yok." /> : <div className="overflow-x-auto"><table className="w-full text-left text-base"><thead className="text-sm uppercase text-slate-500"><tr><th className="p-4">Tip</th><th className="p-4">Symbol</th><th className="p-4">Miktar</th><th className="p-4">Fiyat</th><th className="p-4">Toplam</th><th className="p-4">Tarih</th><th className="p-4">Durum</th></tr></thead><tbody>{filtered.map((t: any) => <tr key={t.id} className="border-t border-line"><td className={`p-4 font-black ${t.type === 'BUY' ? 'text-accent' : 'text-danger'}`}>{t.type}</td><td className="p-4">{t.symbol}</td><td className="p-4">{t.amount}</td><td className="p-4">{formatCurrency(t.price)}</td><td className="p-4">{formatCurrency(t.total)}</td><td className="p-4 text-slate-300">{new Date(t.createdAt).toLocaleString()}</td><td className="p-4">{t.status}</td></tr>)}</tbody></table></div>}
  </section>;
}

function SettingsPage({ user, onSaved, setNotice }: { user?: UserDto; onSaved: (user: UserDto) => void; setNotice: (notice: { type: 'success' | 'error'; message: string }) => void }) {
  const [profile, setProfile] = useState(() => ({ fullName: user?.fullName ?? '', email: user?.email ?? '', userName: user?.userName ?? '', preferredCurrency: user?.preferredCurrency ?? 'USDT', themePreference: user?.themePreference ?? 'dark' }));
  const [passwords, setPasswords] = useState({ currentPassword: '', newPassword: '' });
  useEffect(() => { if (user) setProfile({ fullName: user.fullName, email: user.email, userName: user.userName, preferredCurrency: user.preferredCurrency, themePreference: user.themePreference }); }, [user]);
  const saveProfile = useMutation({ mutationFn: () => updateMe(profile), onSuccess: (saved) => { onSaved(saved); setNotice({ type: 'success', message: 'Ayarlar guncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Ayarlar kaydedilemedi.' }) });
  const savePassword = useMutation({ mutationFn: () => changePassword(passwords), onSuccess: () => { setPasswords({ currentPassword: '', newPassword: '' }); setNotice({ type: 'success', message: 'Sifre guncellendi.' }); }, onError: (e) => setNotice({ type: 'error', message: e instanceof Error ? e.message : 'Sifre guncellenemedi.' }) });
  return <section className="grid gap-6 xl:grid-cols-2"><div className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><h1 className="mb-6 text-2xl font-black">Ayarlar</h1><div className="space-y-4"><Input label="Ad soyad" value={profile.fullName} onChange={(fullName) => setProfile({ ...profile, fullName })} /><Input label="Email" value={profile.email} onChange={(email) => setProfile({ ...profile, email })} /><Input label="Kullanici adi" value={profile.userName} onChange={(userName) => setProfile({ ...profile, userName })} /><Input label="Para birimi" value={profile.preferredCurrency} onChange={(preferredCurrency) => setProfile({ ...profile, preferredCurrency })} /><Input label="Tema" value={profile.themePreference} onChange={(themePreference) => setProfile({ ...profile, themePreference })} /><button onClick={() => saveProfile.mutate()} className="rounded-2xl bg-accent px-6 py-4 text-base font-black text-white">Kaydet</button></div></div><div className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><h2 className="mb-6 text-xl font-black">Sifre Degistir</h2><div className="space-y-4"><Input label="Mevcut sifre" type="password" value={passwords.currentPassword} onChange={(currentPassword) => setPasswords({ ...passwords, currentPassword })} /><Input label="Yeni sifre" type="password" value={passwords.newPassword} onChange={(newPassword) => setPasswords({ ...passwords, newPassword })} /><button onClick={() => savePassword.mutate()} className="rounded-2xl bg-cyan-500 px-6 py-4 text-base font-black text-white">Sifreyi Guncelle</button></div></div></section>;
}

function AssetList({ assets, prices }: any) {
  return <div className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><h2 className="mb-5 text-xl font-black">Varliklarim</h2><div className="space-y-4">{assets.length === 0 && <State text="Henuz varlik yok." />}{assets.map((asset: any) => <div key={asset.id} className="grid gap-4 rounded-2xl border border-line bg-white/[0.04] p-5 text-sm sm:grid-cols-4"><div><div className="text-lg font-black">{coinShort(asset.symbol)}</div><div className="text-slate-500">{asset.symbol}</div></div><Info label="Miktar" value={asset.amount} /><Info label="Ortalama" value={formatCurrency(asset.averageCost)} /><Info label="Deger" value={formatCurrency(asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost))} /></div>)}</div></div>;
}

function Brand() { return <div className="flex items-center gap-3"><div className="grid h-12 w-12 place-items-center rounded-2xl bg-accent/15 text-accent"><Activity size={26} /></div><div><div className="text-xl font-black tracking-wide">TRade<span className="text-accent">Turk</span></div><div className="text-xs text-slate-500">Pro Trading Desk</div></div></div>; }
function UserPanel({ user, onLogout }: { user?: UserDto; onLogout: () => void }) { return <div className="absolute bottom-7 left-7 right-7 rounded-2xl border border-line bg-white/[0.04] p-5"><div className="mb-4 flex items-center gap-3"><div className="grid h-12 w-12 place-items-center rounded-2xl bg-slate-700 text-base font-black">{user?.fullName?.slice(0, 2).toUpperCase() ?? 'TT'}</div><div><div className="text-base font-black">{user?.fullName ?? 'TRadeTurk'}</div><div className="text-xs text-slate-500">{user?.email}</div></div></div><button onClick={onLogout} className="flex w-full items-center justify-center gap-2 rounded-xl border border-line py-3 text-sm font-bold text-slate-300"><LogOut size={18} /> Cikis</button></div>; }
function Metric({ title, value, sub }: { title: string; value: string; sub: string }) { return <article className="rounded-2xl border border-line bg-panel p-6 shadow-xl"><div className="text-base font-semibold text-slate-400">{title}</div><div className="mt-3 text-2xl font-black text-white">{value}</div><div className="mt-3 text-sm text-cyan-300">{sub}</div></article>; }
function Row({ label, value, strong }: { label: string; value: string; strong?: boolean }) { return <div className={`flex justify-between ${strong ? 'border-t border-line pt-3 text-base font-black text-white' : 'text-slate-400'}`}><span>{label}</span><span>{value}</span></div>; }
function Info({ label, value }: { label: string; value: string | number }) { return <div><div className="text-xs text-slate-500">{label}</div><div className="text-base font-bold">{value}</div></div>; }
function State({ text }: { text: string }) { return <div className="rounded-2xl border border-line bg-white/[0.03] p-6 text-base text-slate-400">{text}</div>; }
function Input({ label, value, onChange, type = 'text' }: { label: string; value: string; onChange: (value: string) => void; type?: string }) { return <label className="block"><span className="mb-2 block text-sm font-semibold text-slate-300">{label}</span><input type={type} value={value} onChange={(event) => onChange(event.target.value)} className="w-full rounded-2xl border border-line bg-panelSoft px-4 py-3 text-base outline-none focus:border-accent/60" /></label>; }
function Toast({ type, message, onClose }: { type: 'success' | 'error'; message: string; onClose: () => void }) { return <div className={`fixed right-5 top-5 z-50 flex max-w-md items-center gap-3 rounded-2xl border p-4 text-sm shadow-xl ${type === 'success' ? 'border-accent/30 bg-accent/15 text-emerald-100' : 'border-danger/30 bg-danger/15 text-rose-100'}`}><CheckCircle2 className={type === 'success' ? 'text-accent' : 'text-danger'} /><span>{message}</span><button onClick={onClose}><X size={18} /></button></div>; }
