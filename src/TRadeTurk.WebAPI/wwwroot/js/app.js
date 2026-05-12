// State Management
const state = {
    prices: {
        BTCUSDT: { price: 0, prevPrice: 0, change: 0 },
        ETHUSDT: { price: 0, prevPrice: 0, change: 0 }
    },
    fiatBalance: 50000.00,
    assets: [],
    transactions: [],
    currentMode: 'buy', // 'buy' or 'sell'
    selectedSymbol: 'BTCUSDT'
};

// SignalR Connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/priceHub")
    .withAutomaticReconnect()
    .build();

// Elements
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

// Initialize
async function init() {
    setupSignalR();
    setupEventListeners();
    updateUI();
}

function setupSignalR() {
    connection.on("ReceivePriceUpdate", (symbol, price) => {
        handlePriceUpdate(symbol, price);
    });

    connection.start()
        .then(() => {
            console.log("SignalR Connected.");
            elements.connectionStatus.classList.add('connected');
            elements.connectionStatus.querySelector('.status-text').innerText = "Canlı Veri Bağlı";
        })
        .catch(err => {
            console.error("SignalR Connection Error: ", err);
            elements.connectionStatus.querySelector('.status-text').innerText = "Bağlantı Hatası";
        });
}

function handlePriceUpdate(symbol, price) {
    if (!state.prices[symbol]) return;

    const prevPrice = state.prices[symbol].price;
    state.prices[symbol].prevPrice = prevPrice;
    state.prices[symbol].price = price;

    // Update Card UI
    const priceEl = document.getElementById(`price-${symbol}`);
    const cardEl = document.getElementById(`card-${symbol}`);
    
    if (priceEl) {
        priceEl.innerText = formatCurrency(price);
        
        // Price animation
        if (price > prevPrice) {
            priceEl.className = 'price-value price-up';
        } else if (price < prevPrice) {
            priceEl.className = 'price-value price-down';
        }

        // Remove animation class after 1s to allow re-trigger
        setTimeout(() => {
            priceEl.classList.remove('price-up', 'price-down');
        }, 1000);
    }

    // Update Trade Summary if selected
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

    elements.tradeAmountInput.addEventListener('input', () => {
        updateTradeSummary();
    });

    elements.tabBuy.addEventListener('click', () => {
        state.currentMode = 'buy';
        elements.tabBuy.classList.add('active');
        elements.tabSell.classList.remove('active');
        elements.executeBtn.innerText = "Satın Alımı Onayla";
        elements.executeBtn.style.background = "linear-gradient(90deg, var(--success), #1bd389)";
        updateTradeSummary();
    });

    elements.tabSell.addEventListener('click', () => {
        state.currentMode = 'sell';
        elements.tabSell.classList.add('active');
        elements.tabBuy.classList.remove('active');
        elements.executeBtn.innerText = "Satışı Onayla";
        elements.executeBtn.style.background = "linear-gradient(90deg, var(--danger), #ff5e71)";
        updateTradeSummary();
    });

    elements.executeBtn.addEventListener('click', () => {
        executeTrade();
    });
}

function updateTradeSummary() {
    const symbol = state.selectedSymbol;
    const amount = parseFloat(elements.tradeAmountInput.value) || 0;
    const price = state.prices[symbol].price;
    const commissionRate = 0.001; // 0.1%

    const subtotal = amount * price;
    const fee = subtotal * commissionRate;
    const total = state.currentMode === 'buy' ? subtotal + fee : subtotal - fee;

    elements.summaryPrice.innerText = formatCurrency(price);
    elements.summaryFee.innerText = formatCurrency(fee);
    elements.summaryTotal.innerText = formatCurrency(total);
}

function executeTrade() {
    const amount = parseFloat(elements.tradeAmountInput.value);
    if (!amount || amount <= 0) {
        showToast("Lütfen geçerli bir miktar girin.", "error");
        return;
    }

    const total = parseFloat(elements.summaryTotal.innerText.replace('$', '').replace(',', ''));
    
    if (state.currentMode === 'buy' && total > state.fiatBalance) {
        showToast("Yetersiz bakiye!", "error");
        return;
    }

    // Simulate API Call
    elements.executeBtn.disabled = true;
    elements.executeBtn.innerText = "İşleniyor...";

    setTimeout(() => {
        // Mock Success
        if (state.currentMode === 'buy') {
            state.fiatBalance -= total;
        } else {
            state.fiatBalance += total;
        }

        showToast(`İşlem Başarılı: ${state.selectedSymbol} ${state.currentMode === 'buy' ? 'Alındı' : 'Satıldı'}`, "success");
        
        elements.totalFiatBalance.innerText = formatCurrency(state.fiatBalance);
        elements.executeBtn.disabled = false;
        elements.executeBtn.innerText = state.currentMode === 'buy' ? "Satın Alımı Onayla" : "Satışı Onayla";
        elements.tradeAmountInput.value = "";
        
        addTransactionToHistory(state.currentMode, state.selectedSymbol, amount, total);
        updateTradeSummary();
    }, 1500);
}

function addTransactionToHistory(type, symbol, amount, total) {
    const list = document.getElementById('transactions-list');
    const emptyState = list.querySelector('.empty-state');
    if (emptyState) emptyState.remove();

    const item = document.createElement('div');
    item.className = 'transaction-item';
    item.innerHTML = `
        <div class="tx-info">
            <span class="tx-type ${type}">${type === 'buy' ? 'ALIŞ' : 'SATIŞ'}</span>
            <span class="tx-asset">${symbol.replace('USDT', '')}</span>
        </div>
        <div class="tx-details">
            <span class="tx-amount">${amount}</span>
            <span class="tx-total">${formatCurrency(total)}</span>
        </div>
    `;
    list.prepend(item);
}

// Helpers
function formatCurrency(value) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
}

function showToast(message, type) {
    // Basic Alert for now, can be improved to a real toast
    alert(message);
}

function updateUI() {
    elements.totalFiatBalance.innerText = formatCurrency(state.fiatBalance);
    elements.unitSpan.innerText = state.selectedSymbol.replace('USDT', '');
}

// Start
init();
