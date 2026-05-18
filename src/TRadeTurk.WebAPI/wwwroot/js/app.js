const DEMO_USER_ID = "11111111-1111-1111-1111-111111111111";

const state = {
    prices: {
        BTCUSDT: { price: 0, prevPrice: 0, change: 0 },
        ETHUSDT: { price: 0, prevPrice: 0, change: 0 }
    },
    fiatBalance: 0,
    assets: [],
    currentMode: 'buy',
    selectedSymbol: 'BTCUSDT',
    isSubmitting: false
};

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/priceHub")
    .withAutomaticReconnect()
    .build();

const elements = {
    connectionStatus: document.getElementById('connection-status'),
    totalFiatBalance: document.getElementById('total-fiat-balance'),
    tradeAmountInput: document.getElementById('trade-amount'),
    tradeAssetSelect: document.getElementById('trade-asset-select'),
    summaryPrice: document.getElementById('summary-price'),
    summaryFee: document.getElementById('summary-fee'),
    summaryTotal: document.getElementById('summary-total'),
    executeBtn: document.getElementById('execute-trade-btn'),
    tabBuy: document.getElementById('tab-buy'),
    tabSell: document.getElementById('tab-sell'),
    unitSpan: document.getElementById('selected-symbol-unit')
};

async function init() {
    setupSignalR();
    setupEventListeners();
    await Promise.all([loadWallet(), loadAssets(), loadInitialPrices()]);
    updateUI();
}

function setupSignalR() {
    connection.on("ReceivePriceUpdate", (symbol, price) => {
        handlePriceUpdate(symbol, Number(price));
    });

    connection.start()
        .then(() => {
            elements.connectionStatus.classList.add('connected');
            elements.connectionStatus.querySelector('.status-text').innerText = "Canli Veri Bagli";
        })
        .catch(err => {
            console.error("SignalR Connection Error: ", err);
            elements.connectionStatus.querySelector('.status-text').innerText = "Baglanti Hatasi";
        });
}

async function loadInitialPrices() {
    await Promise.all(Object.keys(state.prices).map(async (symbol) => {
        try {
            const response = await fetch(`/api/prices/${symbol}`);
            if (!response.ok) return;

            const data = await response.json();
            handlePriceUpdate(data.symbol, Number(data.price));
        } catch (error) {
            console.error("Price load failed", error);
        }
    }));
}

async function loadWallet() {
    const response = await fetch(`/api/wallet/${DEMO_USER_ID}`);
    if (!response.ok) {
        showToast("Cuzdan bilgisi alinamadi. Migration ve seed verisini kontrol edin.", "error");
        return;
    }

    const wallet = await response.json();
    state.fiatBalance = Number(wallet.fiatBalance);
    updateUI();
}

async function loadAssets() {
    const response = await fetch(`/api/assets/${DEMO_USER_ID}`);
    if (!response.ok) return;

    state.assets = await response.json();
}

function handlePriceUpdate(symbol, price) {
    if (!state.prices[symbol]) return;

    const prevPrice = state.prices[symbol].price;
    state.prices[symbol].prevPrice = prevPrice;
    state.prices[symbol].price = price;

    const priceEl = document.getElementById(`price-${symbol}`);
    if (priceEl) {
        priceEl.innerText = formatCurrency(price);

        if (price > prevPrice) {
            priceEl.className = 'price-value price-up';
        } else if (price < prevPrice) {
            priceEl.className = 'price-value price-down';
        }

        setTimeout(() => {
            priceEl.classList.remove('price-up', 'price-down');
        }, 1000);
    }

    if (symbol === state.selectedSymbol) {
        updateTradeSummary();
    }
}

function setupEventListeners() {
    elements.tradeAssetSelect.addEventListener('change', (e) => {
        state.selectedSymbol = e.target.value;
        elements.unitSpan.innerText = state.selectedSymbol.replace('USDT', '');
        updateTradeSummary();
    });

    elements.tradeAmountInput.addEventListener('input', updateTradeSummary);

    elements.tabBuy.addEventListener('click', () => setTradeMode('buy'));
    elements.tabSell.addEventListener('click', () => setTradeMode('sell'));
    elements.executeBtn.addEventListener('click', executeTrade);
}

function setTradeMode(mode) {
    state.currentMode = mode;
    elements.tabBuy.classList.toggle('active', mode === 'buy');
    elements.tabSell.classList.toggle('active', mode === 'sell');
    setSubmitButton(false);
    elements.executeBtn.style.background = mode === 'buy'
        ? "linear-gradient(90deg, var(--success), #1bd389)"
        : "linear-gradient(90deg, var(--danger), #ff5e71)";
    updateTradeSummary();
}

function updateTradeSummary() {
    const symbol = state.selectedSymbol;
    const amount = Number(elements.tradeAmountInput.value) || 0;
    const price = state.prices[symbol].price;
    const commissionRate = 0.001;

    const subtotal = amount * price;
    const fee = subtotal * commissionRate;
    const total = state.currentMode === 'buy' ? subtotal + fee : subtotal - fee;

    elements.summaryPrice.innerText = formatCurrency(price);
    elements.summaryFee.innerText = formatCurrency(fee);
    elements.summaryTotal.innerText = formatCurrency(total);
}

async function executeTrade() {
    const amount = Number(elements.tradeAmountInput.value);
    const symbol = state.selectedSymbol;
    const requestedPrice = state.prices[symbol].price;

    if (!amount || amount <= 0) {
        showToast("Lutfen gecerli bir miktar girin.", "error");
        return;
    }

    if (!requestedPrice || requestedPrice <= 0) {
        showToast("Fiyat bilgisi henuz hazir degil.", "error");
        return;
    }

    setSubmitButton(true);

    try {
        const endpoint = state.currentMode === 'buy' ? '/api/trade/buy' : '/api/trade/sell';
        const response = await fetch(endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userId: DEMO_USER_ID,
                symbol,
                amount,
                requestedPrice
            })
        });

        const result = await response.json();
        if (!response.ok || !result.isSuccess) {
            showToast(result.message || "Islem basarisiz.", "error");
            return;
        }

        showToast(result.message, "success");
        elements.tradeAmountInput.value = "";
        addTransactionToHistory(state.currentMode, symbol, amount, result.executedPrice, result.commissionUsed);

        await Promise.all([loadWallet(), loadAssets()]);
        updateTradeSummary();
    } catch (error) {
        console.error("Trade failed", error);
        showToast("Backend isleminde hata olustu.", "error");
    } finally {
        setSubmitButton(false);
    }
}

function setSubmitButton(isLoading) {
    state.isSubmitting = isLoading;
    elements.executeBtn.disabled = isLoading;
    elements.executeBtn.innerText = isLoading
        ? "Isleniyor..."
        : (state.currentMode === 'buy' ? "Satin Alimi Onayla" : "Satisi Onayla");
}

function addTransactionToHistory(type, symbol, amount, executedPrice, commission) {
    const list = document.getElementById('transactions-list');
    const emptyState = list.querySelector('.empty-state');
    if (emptyState) emptyState.remove();

    const item = document.createElement('div');
    item.className = 'transaction-item';
    item.innerHTML = `
        <div class="tx-info">
            <span class="tx-type ${type}">${type === 'buy' ? 'ALIS' : 'SATIS'}</span>
            <span class="tx-asset">${symbol.replace('USDT', '')}</span>
        </div>
        <div class="tx-details">
            <span class="tx-amount">${amount}</span>
            <span class="tx-total">${formatCurrency(amount * executedPrice)} (${formatCurrency(commission)} kom.)</span>
        </div>
    `;
    list.prepend(item);
}

function formatCurrency(value) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(Number(value) || 0);
}

function showToast(message, type) {
    alert(`${type === "success" ? "Basarili" : "Hata"}: ${message}`);
}

function updateUI() {
    elements.totalFiatBalance.innerText = formatCurrency(state.fiatBalance);
    elements.unitSpan.innerText = state.selectedSymbol.replace('USDT', '');
}

init();
