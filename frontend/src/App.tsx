import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Activity, Bell, History, LayoutDashboard, Loader2, Search, Settings, Wallet, Wifi } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { API_BASE_URL, DEMO_USER_ID } from './config';
import { executeTrade, getAssets, getWallet } from './api';
import type { PriceState, TradeMode } from './types';

const symbols = ['BTCUSDT', 'ETHUSDT'] as const;

const initialPrices: Record<string, PriceState> = {
  BTCUSDT: { symbol: 'BTCUSDT', price: 0, previousPrice: 0 },
  ETHUSDT: { symbol: 'ETHUSDT', price: 0, previousPrice: 0 }
};

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value || 0);

const coinName = (symbol: string) => (symbol === 'BTCUSDT' ? 'Bitcoin' : 'Ethereum');
const coinShort = (symbol: string) => symbol.replace('USDT', '');

export function App() {
  const queryClient = useQueryClient();
  const [prices, setPrices] = useState(initialPrices);
  const [connectionLabel, setConnectionLabel] = useState('Baglaniyor...');
  const [mode, setMode] = useState<TradeMode>('buy');
  const [selectedSymbol, setSelectedSymbol] = useState('BTCUSDT');
  const [amount, setAmount] = useState('');
  const [notice, setNotice] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  const walletQuery = useQuery({
    queryKey: ['wallet', DEMO_USER_ID],
    queryFn: () => getWallet(DEMO_USER_ID)
  });

  const assetsQuery = useQuery({
    queryKey: ['assets', DEMO_USER_ID],
    queryFn: () => getAssets(DEMO_USER_ID)
  });

  const selectedPrice = prices[selectedSymbol]?.price ?? 0;
  const numericAmount = Number(amount) || 0;
  const fee = numericAmount * selectedPrice * 0.001;
  const gross = numericAmount * selectedPrice;
  const total = mode === 'buy' ? gross + fee : gross - fee;

  const tradeMutation = useMutation({
    mutationFn: () =>
      executeTrade(mode, {
        userId: DEMO_USER_ID,
        symbol: selectedSymbol,
        amount: numericAmount,
        requestedPrice: selectedPrice
      }),
    onSuccess: async (result) => {
      setNotice({ type: result.isSuccess ? 'success' : 'error', message: result.message });
      setAmount('');
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['wallet', DEMO_USER_ID] }),
        queryClient.invalidateQueries({ queryKey: ['assets', DEMO_USER_ID] })
      ]);
    },
    onError: (error) => {
      const message = error instanceof Error ? error.message : 'Islem basarisiz oldu.';
      setNotice({ type: 'error', message });
    }
  });

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/priceHub`)
      .withAutomaticReconnect()
      .build();

    connection.on('ReceivePriceUpdate', (symbol: string, price: number) => {
      setPrices((current) => ({
        ...current,
        [symbol]: {
          symbol,
          previousPrice: current[symbol]?.price ?? 0,
          price: Number(price)
        }
      }));
    });

    connection
      .start()
      .then(() => setConnectionLabel(connection.state === HubConnectionState.Connected ? 'Canli Veri Bagli' : 'Baglaniyor...'))
      .catch(() => setConnectionLabel('Baglanti Hatasi'));

    return () => {
      void connection.stop();
    };
  }, []);

  const portfolioValue = useMemo(() => {
    return (assetsQuery.data ?? []).reduce((sum, asset) => sum + asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost), 0);
  }, [assetsQuery.data, prices]);

  function submitTrade() {
    setNotice(null);

    if (!numericAmount || numericAmount <= 0) {
      setNotice({ type: 'error', message: 'Lutfen gecerli bir miktar girin.' });
      return;
    }

    if (!selectedPrice || selectedPrice <= 0) {
      setNotice({ type: 'error', message: 'Canli fiyat henuz hazir degil.' });
      return;
    }

    tradeMutation.mutate();
  }

  return (
    <div className="min-h-screen bg-ink text-slate-100">
      <aside className="fixed inset-y-0 left-0 hidden w-72 border-r border-line bg-[#0b1020] px-6 py-7 lg:block">
        <div className="mb-10 flex items-center gap-3">
          <div className="grid h-11 w-11 place-items-center rounded-lg bg-accent/15 text-accent">
            <Activity size={24} />
          </div>
          <div>
            <div className="text-xl font-black tracking-wide">TRade<span className="text-accent">Turk</span></div>
            <div className="text-xs text-slate-500">Pro Trading Desk</div>
          </div>
        </div>

        <nav className="space-y-2 text-sm">
          {[
            [LayoutDashboard, 'Dashboard'],
            [Wallet, 'Cuzdanim'],
            [History, 'Islem Gecmisi'],
            [Settings, 'Ayarlar']
          ].map(([Icon, label]) => (
            <button key={label as string} className="flex w-full items-center gap-3 rounded-lg px-3 py-3 text-left text-slate-300 transition hover:bg-white/5 first:bg-accent/10 first:text-accent">
              <Icon size={18} />
              <span>{label as string}</span>
            </button>
          ))}
        </nav>

        <div className="absolute bottom-7 left-6 right-6 rounded-lg border border-line bg-white/[0.03] p-4">
          <div className="flex items-center gap-3">
            <div className="grid h-10 w-10 place-items-center rounded-lg bg-slate-700 font-bold">HD</div>
            <div>
              <div className="text-sm font-semibold">Hasan Dogan</div>
              <div className="text-xs text-slate-500">Demo user</div>
            </div>
          </div>
        </div>
      </aside>

      <main className="lg:pl-72">
        <header className="sticky top-0 z-10 border-b border-line bg-ink/85 px-5 py-4 backdrop-blur lg:px-8">
          <div className="flex items-center justify-between gap-4">
            <div className="flex min-w-0 flex-1 items-center gap-3 rounded-lg border border-line bg-white/[0.03] px-4 py-3">
              <Search size={18} className="text-slate-500" />
              <input className="w-full bg-transparent text-sm outline-none placeholder:text-slate-600" placeholder="Varlik ara (BTC, ETH, SOL...)" />
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-2 rounded-lg border border-line bg-white/[0.03] px-3 py-2 text-xs text-slate-300 sm:flex">
                <Wifi size={15} className="text-accent" />
                {connectionLabel}
              </div>
              <button className="grid h-10 w-10 place-items-center rounded-lg border border-line bg-white/[0.03]">
                <Bell size={18} />
              </button>
            </div>
          </div>
        </header>

        <div className="mx-auto max-w-7xl px-5 py-7 lg:px-8">
          <section className="mb-7 grid gap-4 lg:grid-cols-[1fr_360px]">
            <div>
              <p className="text-sm text-slate-500">Canli portfoy</p>
              <h1 className="mt-2 text-3xl font-black text-white md:text-5xl">TRadeTurk Dashboard</h1>
            </div>
            <div className="rounded-lg border border-line bg-panel p-5">
              <div className="text-sm text-slate-400">Toplam Sanal Bakiye</div>
              <div className="mt-2 text-3xl font-black text-white">{formatCurrency(walletQuery.data?.fiatBalance ?? 0)}</div>
              <div className="mt-3 text-sm text-accent">Portfoy: {formatCurrency(portfolioValue)}</div>
            </div>
          </section>

          {notice && (
            <div className={`mb-5 rounded-lg border px-4 py-3 text-sm ${notice.type === 'success' ? 'border-accent/30 bg-accent/10 text-accent' : 'border-danger/30 bg-danger/10 text-rose-200'}`}>
              {notice.message}
            </div>
          )}

          <section className="mb-7">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-xl font-bold">Canli Piyasalar</h2>
              <span className="text-sm text-slate-500">SignalR feed</span>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              {symbols.map((symbol) => {
                const price = prices[symbol];
                const direction = price.price >= price.previousPrice ? 'text-accent' : 'text-danger';
                return (
                  <article key={symbol} className="rounded-lg border border-line bg-panel p-5">
                    <div className="flex items-start justify-between">
                      <div className="flex items-center gap-3">
                        <div className="grid h-12 w-12 place-items-center rounded-lg bg-white/5 text-lg font-black">{coinShort(symbol)[0]}</div>
                        <div>
                          <div className="font-bold">{coinShort(symbol)}</div>
                          <div className="text-sm text-slate-500">{coinName(symbol)}</div>
                        </div>
                      </div>
                      <div className={`text-right text-xl font-black ${direction}`}>{formatCurrency(price.price)}</div>
                    </div>
                    <div className="mt-5 h-16 rounded-lg bg-[linear-gradient(135deg,rgba(34,197,94,0.20),rgba(59,130,246,0.08))]" />
                  </article>
                );
              })}
            </div>
          </section>

          <section className="grid gap-5 xl:grid-cols-[420px_1fr]">
            <div className="rounded-lg border border-line bg-panel p-5">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-xl font-bold">Hizli Islem</h2>
                {tradeMutation.isPending && <Loader2 className="animate-spin text-accent" size={18} />}
              </div>

              <div className="mb-5 grid grid-cols-2 rounded-lg bg-black/20 p-1">
                {(['buy', 'sell'] as TradeMode[]).map((item) => (
                  <button key={item} onClick={() => setMode(item)} className={`rounded-md py-2 text-sm font-bold ${mode === item ? 'bg-white/10 text-white' : 'text-slate-500'}`}>
                    {item === 'buy' ? 'Alis' : 'Satis'}
                  </button>
                ))}
              </div>

              <label className="mb-2 block text-sm text-slate-400">Varlik</label>
              <select value={selectedSymbol} onChange={(event) => setSelectedSymbol(event.target.value)} className="mb-4 w-full rounded-lg border border-line bg-panelSoft px-3 py-3 outline-none">
                {symbols.map((symbol) => <option key={symbol} value={symbol}>{coinName(symbol)} ({coinShort(symbol)})</option>)}
              </select>

              <label className="mb-2 block text-sm text-slate-400">Miktar</label>
              <div className="mb-5 flex items-center rounded-lg border border-line bg-panelSoft px-3">
                <input value={amount} onChange={(event) => setAmount(event.target.value)} type="number" min="0" step="0.00000001" className="w-full bg-transparent py-3 outline-none" placeholder="0.00" />
                <span className="text-sm font-bold text-slate-400">{coinShort(selectedSymbol)}</span>
              </div>

              <div className="mb-5 space-y-3 rounded-lg bg-black/20 p-4 text-sm">
                <div className="flex justify-between text-slate-400"><span>Birim Fiyat</span><span>{formatCurrency(selectedPrice)}</span></div>
                <div className="flex justify-between text-slate-400"><span>Komisyon (0.1%)</span><span>{formatCurrency(fee)}</span></div>
                <div className="flex justify-between border-t border-line pt-3 font-bold text-white"><span>Toplam</span><span>{formatCurrency(total)}</span></div>
              </div>

              <button disabled={tradeMutation.isPending} onClick={submitTrade} className={`w-full rounded-lg py-3 font-black text-white transition disabled:cursor-not-allowed disabled:opacity-60 ${mode === 'buy' ? 'bg-accent hover:bg-green-400' : 'bg-danger hover:bg-rose-400'}`}>
                {tradeMutation.isPending ? 'Isleniyor...' : mode === 'buy' ? 'Satin Alimi Onayla' : 'Satisi Onayla'}
              </button>
            </div>

            <div className="rounded-lg border border-line bg-panel p-5">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-xl font-bold">Varliklarim</h2>
                <span className="text-sm text-slate-500">{assetsQuery.isFetching ? 'Guncelleniyor...' : 'Canli'}</span>
              </div>
              <div className="space-y-3">
                {(assetsQuery.data ?? []).length === 0 && <div className="rounded-lg border border-line p-5 text-slate-500">Henuz varlik yok.</div>}
                {(assetsQuery.data ?? []).map((asset) => (
                  <div key={asset.id} className="grid gap-3 rounded-lg border border-line bg-white/[0.03] p-4 sm:grid-cols-4">
                    <div>
                      <div className="font-bold">{coinShort(asset.symbol)}</div>
                      <div className="text-xs text-slate-500">{asset.symbol}</div>
                    </div>
                    <div>
                      <div className="text-xs text-slate-500">Miktar</div>
                      <div className="font-semibold">{asset.amount}</div>
                    </div>
                    <div>
                      <div className="text-xs text-slate-500">Ortalama</div>
                      <div className="font-semibold">{formatCurrency(asset.averageCost)}</div>
                    </div>
                    <div>
                      <div className="text-xs text-slate-500">Deger</div>
                      <div className="font-semibold">{formatCurrency(asset.amount * (prices[asset.symbol]?.price ?? asset.averageCost))}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </section>
        </div>
      </main>
    </div>
  );
}
