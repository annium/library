# Futures (USDⓈ-M) API reference — extracted from llms-full.txt

> Extracted 2026-09-18 from `https://developers.binance.com/en/docs/llms-full.txt`
> (8229421 bytes, sha256 aed9aa2c739cea2452f6b00263563f5009c6a64f6ad81d84a32f4e87dc3490e9),
> lines 204286-205569: the three USDⓈ-M sections of its "API Reference" chapter.
>
> This is the per-endpoint reference the 2026-09-01 run could not retrieve at all. The rendered
> catalog pages still return the site's HTML shell; this text is the same material served whole.

### Futures (USDⓈ-M) REST API (1.0.0)

Access market data, manage accounts, and trade USDⓈ-M perpetual futures.


#### account

##### `GET /fapi/v2/account`

Account Information V2 (USER_DATA)

Get current account information. User in single-asset/ multi-assets mode will see different value, see comments in response section for detail.

Operation ID: `accountInformationV2`

##### `GET /fapi/v3/account`

Account Information V3 (USER_DATA)

Get current account information. User in single-asset/ multi-assets mode will see different value, see comments in response section for detail.

Operation ID: `accountInformationV3`

##### `GET /fapi/v2/balance`

Futures Account Balance V2 (USER_DATA)

Query account balance information.

Operation ID: `futuresAccountBalanceV2`

##### `GET /fapi/v3/balance`

Futures Account Balance V3 (USER_DATA)

Query account balance information.

Operation ID: `futuresAccountBalanceV3`

##### `GET /fapi/v1/accountConfig`

Futures Account Configuration (USER_DATA)

Query account configuration

Operation ID: `futuresAccountConfiguration`

##### `GET /fapi/v1/apiTradingStatus`

Futures Trading Quantitative Rules Indicators (USER_DATA)

Futures trading quantitative rules indicators, for more information on this, please refer to the [Futures Trading Quantitative Rules](https://www.binance.com/en/support/faq/4f462ebe6ff445d4a170be7d9e897272)

Operation ID: `futuresTradingQuantitativeRulesIndicators`

##### `GET /fapi/v1/feeBurn`

Get BNB Burn Status (USER_DATA)

Get user's BNB Fee Discount (Fee Discount On or Fee Discount Off )

Operation ID: `getBnbBurnStatus`

##### `POST /fapi/v1/feeBurn`

Toggle BNB Burn On Futures Trade (TRADE)

Change user's BNB Fee Discount (Fee Discount On or Fee Discount Off ) on ***EVERY symbol***

Operation ID: `toggleBnbBurnOnFuturesTrade`

##### `GET /fapi/v1/multiAssetsMargin`

Get Current Multi-Assets Mode (USER_DATA)

Get user's Multi-Assets mode (Multi-Assets Mode or Single-Asset Mode) on ***Every symbol***

Operation ID: `getCurrentMultiAssetsMode`

##### `GET /fapi/v1/positionSide/dual`

Get Current Position Mode (USER_DATA)

Get user's position mode (Hedge Mode or One-way Mode ) on ***EVERY symbol***

Operation ID: `getCurrentPositionMode`

##### `GET /fapi/v1/order/asyn`

Get Download Id For Futures Order History (USER_DATA)

Get Download Id For Futures Order History

Operation ID: `getDownloadIdForFuturesOrderHistory`

##### `GET /fapi/v1/trade/asyn`

Get Download Id For Futures Trade History (USER_DATA)

Get download id for futures trade history

Operation ID: `getDownloadIdForFuturesTradeHistory`

##### `GET /fapi/v1/income/asyn`

Get Download Id For Futures Transaction History (USER_DATA)

Get download id for futures transaction history

Operation ID: `getDownloadIdForFuturesTransactionHistory`

##### `GET /fapi/v1/order/asyn/id`

Get Futures Order History Download Link by Id (USER_DATA)

Get futures order history download link by Id

Operation ID: `getFuturesOrderHistoryDownloadLinkById`

##### `GET /fapi/v1/trade/asyn/id`

Get Futures Trade Download Link by Id (USER_DATA)

Get futures trade download link by Id

Operation ID: `getFuturesTradeDownloadLinkById`

##### `GET /fapi/v1/income/asyn/id`

Get Futures Transaction History Download Link by Id (USER_DATA)

Get futures transaction history download link by Id

Operation ID: `getFuturesTransactionHistoryDownloadLinkById`

##### `GET /fapi/v1/income`

Get Income History (USER_DATA)

Query income history

Operation ID: `getIncomeHistory`

##### `GET /fapi/v1/leverageBracket`

Notional and Leverage Brackets (USER_DATA)

Query user notional and leverage bracket on speicfic symbol

Operation ID: `notionalAndLeverageBrackets`

##### `GET /fapi/v1/rateLimit/order`

Query User Rate Limit (USER_DATA)

Query User Rate Limit

Operation ID: `queryUserRateLimit`

##### `GET /fapi/v1/symbolConfig`

Symbol Configuration (USER_DATA)

Get current account symbol configuration.

Operation ID: `symbolConfiguration`

##### `GET /fapi/v1/commissionRate`

User Commission Rate (USER_DATA)

Get User Commission Rate

Operation ID: `userCommissionRate`


#### trade

##### `POST /fapi/v1/multiAssetsMargin`

Change Multi-Assets Mode (TRADE)

Change user's Multi-Assets mode (Multi-Assets Mode or Single-Asset Mode) on ***Every symbol***

Operation ID: `changeMultiAssetsMode`

##### `POST /fapi/v1/positionSide/dual`

Change Position Mode (TRADE)

Change user's position mode (Hedge Mode or One-way Mode ) on ***EVERY symbol***.

**After CM migration**, UM and CM share the **same** `dualSidePosition` setting. Calling this endpoint flips both UM and CM at once. If either side has any open order or open position, the change is rejected:
- `-4067` (open orders exist)
- `-4068` (open position exists)

Operation ID: `changePositionMode`

##### `GET /fapi/v1/userTrades`

Account Trade List (USER_DATA)

Get trades for a specific account and symbol.

Operation ID: `accountTradeList`

##### `GET /fapi/v1/allOrders`

All Orders (USER_DATA)

Get all account orders; active, canceled, or filled.

- These orders will not be found:
  - order status is `CANCELED` or `EXPIRED` **AND** order has NO filled trade **AND** created time + 3 days < current time
  - order create time + 90 days < current time

Operation ID: `allOrders`

##### `POST /fapi/v1/countdownCancelAll`

Auto-Cancel All Open Orders (TRADE)

Cancel all open orders of the specified symbol at the end of the specified countdown.

The endpoint should be called repeatedly as heartbeats so that the existing countdown time can be canceled and
replaced by a new one.

Example usage:

Call this endpoint at 30s intervals with an countdownTime of 120000 (120s).
If this endpoint is not called within 120 seconds, all your orders of the specified symbol will be automatically
canceled.
If this endpoint is called with an countdownTime of 0, the countdown timer will be stopped.

The system will check all countdowns **approximately every 10 milliseconds**, so please note that sufficient
redundancy should be considered when using this function. We do not recommend setting the countdown time to be
too precise or too small.

Operation ID: `autoCancelAllOpenOrders`

##### `GET /fapi/v1/algoOrder`

Query Algo Order (USER_DATA)

Check the status of an algo (conditional) order, such as TP/SL (Take Profit / Stop Loss) or trailing stop orders on USD-M Futures.

* These orders will not be found:
  * order status is `CANCELED` or `EXPIRED` **AND** order has NO filled trade **AND** created time + 3 days < current time
  * order create time + 90 days < current time

Operation ID: `queryAlgoOrder`

##### `POST /fapi/v1/algoOrder`

New Algo Order (TRADE)

Send in a new algo (conditional) order. Use this endpoint to place **TP/SL (Take Profit / Stop Loss)** and trailing stop orders on USD-M Futures. Supported order types under `algoType=CONDITIONAL` are `STOP_MARKET`, `TAKE_PROFIT_MARKET`, `STOP`, `TAKE_PROFIT`, and `TRAILING_STOP_MARKET`.

Operation ID: `newAlgoOrder`

##### `DELETE /fapi/v1/algoOrder`

Cancel Algo Order (TRADE)

Cancel an active algo (conditional) order, including TP/SL (Take Profit / Stop Loss) and trailing stop orders on USD-M Futures.

Operation ID: `cancelAlgoOrder`

##### `DELETE /fapi/v1/algoOpenOrders`

Cancel All Algo Open Orders (TRADE)

Cancel all open algo (conditional) orders on a symbol, including TP/SL (Take Profit / Stop Loss) and trailing stop orders on USD-M Futures.

Operation ID: `cancelAllAlgoOpenOrders`

##### `DELETE /fapi/v1/allOpenOrders`

Cancel All Open Orders (TRADE)

Cancel All Open Orders

Operation ID: `cancelAllOpenOrders`

##### `PUT /fapi/v1/batchOrders`

Modify Multiple Orders (TRADE)

Operation ID: `modifyMultipleOrders`

##### `POST /fapi/v1/batchOrders`

Place Multiple Orders (TRADE)

Place Multiple Orders

Operation ID: `placeMultipleOrders`

##### `DELETE /fapi/v1/batchOrders`

Cancel Multiple Orders (TRADE)

Cancel Multiple Orders

Operation ID: `cancelMultipleOrders`

##### `GET /fapi/v1/order`

Query Order (USER_DATA)

Check an order's status.

* These orders will not be found:
  * order status is `CANCELED` or `EXPIRED` **AND** order has NO filled trade **AND** created time + 3 days < current time
  * order create time + 90 days < current time

Operation ID: `queryOrder`

##### `PUT /fapi/v1/order`

Modify Order (TRADE)

Order modify function, currently only LIMIT order modification is supported, modified orders will be reordered in the match queue

Operation ID: `modifyOrder`

##### `POST /fapi/v1/order`

New Order (TRADE)

Send in a new order.

Operation ID: `newOrder`

##### `DELETE /fapi/v1/order`

Cancel Order (TRADE)

Cancel an active order.

Operation ID: `cancelOrder`

##### `POST /fapi/v1/leverage`

Change Initial Leverage (TRADE)

Change user's initial leverage of specific symbol market.

Operation ID: `changeInitialLeverage`

##### `POST /fapi/v1/marginType`

Change Margin Type (TRADE)

Change symbol level margin type

Operation ID: `changeMarginType`

##### `GET /fapi/v1/openAlgoOrders`

Current All Algo Open Orders (USER_DATA)

Get all open algo (conditional) orders on a symbol, including TP/SL (Take Profit / Stop Loss) and trailing stop orders on USD-M Futures.

Operation ID: `currentAllAlgoOpenOrders`

##### `GET /fapi/v1/openOrders`

Current All Open Orders (USER_DATA)

Get all open orders on a symbol.

Operation ID: `currentAllOpenOrders`

##### `POST /fapi/v1/stock/contract`

Futures TradFi Perps Contract (USER_DATA)

Sign TradFi-Perps agreement contract

Operation ID: `futuresTradfiPerpsContract`

##### `GET /fapi/v1/orderAmendment`

Get Order Modify History (USER_DATA)

Get order modification history

Operation ID: `getOrderModifyHistory`

##### `GET /fapi/v1/positionMargin/history`

Get Position Margin Change History (TRADE)

Get Position Margin Change History

Operation ID: `getPositionMarginChangeHistory`

##### `POST /fapi/v1/positionMargin`

Modify Isolated Position Margin (TRADE)

Modify Isolated Position Margin

Operation ID: `modifyIsolatedPositionMargin`

##### `GET /fapi/v1/adlQuantile`

Position ADL Quantile Estimation (USER_DATA)

Position ADL Quantile Estimation

* Values update every 30s.
* Values 0, 1, 2, 3, 4 shows the queue position and possibility of ADL from low to high.
* For positions of the symbol are in One-way Mode or isolated margined in Hedge Mode, "LONG", "SHORT", and "BOTH" will be returned to show the positions' adl quantiles of different position sides.
* If the positions of the symbol are crossed margined in Hedge Mode:
  * "HEDGE" as a sign will be returned instead of "BOTH";
  * A same value caculated on unrealized pnls on long and short sides' positions will be shown for "LONG" and "SHORT" when there are positions in both of long and short sides.

Operation ID: `positionAdlQuantileEstimation`

##### `GET /fapi/v2/positionRisk`

Position Information V2 (USER_DATA)

Get current position information.

Operation ID: `positionInformationV2`

##### `GET /fapi/v3/positionRisk`

Position Information V3 (USER_DATA)

Get current position information(only symbol that has position or open
orders will be returned).

Operation ID: `positionInformationV3`

##### `GET /fapi/v1/allAlgoOrders`

Query All Algo Orders (USER_DATA)

Get all algo (conditional) orders — active, CANCELED, TRIGGERED, or FINISHED — including TP/SL (Take Profit / Stop Loss) and trailing stop orders on USD-M Futures.

* These orders will not be found:
  * order status is `CANCELED` or `EXPIRED` **AND** order has NO filled trade **AND** created time + 3 days < current time
  * order create time + 90 days < current time

Operation ID: `queryAllAlgoOrders`

##### `GET /fapi/v1/openOrder`

Query Current Open Order (USER_DATA)

Query open order

Operation ID: `queryCurrentOpenOrder`

##### `POST /fapi/v1/order/test`

Test Order (TRADE)

Testing order request, this order will not be submitted to matching engine

Operation ID: `testOrder`

##### `GET /fapi/v1/forceOrders`

User's Force Orders (USER_DATA)

Query user's Force Orders

Operation ID: `usersForceOrders`


#### convert

##### `POST /fapi/v1/convert/acceptQuote`

Accept the offered quote (USER_DATA)

Accept the offered quote by quote ID.

Operation ID: `acceptTheOfferedQuote`

##### `GET /fapi/v1/convert/exchangeInfo`

List All Convert Pairs

Query for all convertible token pairs and the tokens’ respective upper/lower limits

Operation ID: `listAllConvertPairs`

##### `GET /fapi/v1/convert/orderStatus`

Order status (USER_DATA)

Query order status by order ID.

Operation ID: `orderStatus`

##### `POST /fapi/v1/convert/getQuote`

Send Quote Request (USER_DATA)

Request a quote for the requested token pairs

Operation ID: `sendQuoteRequest`


#### market-data

##### `GET /fapi/v1/symbolAdlRisk`

ADL Risk

Query the symbol-level ADL risk rating.

The ADL risk rating measures the likelihood of ADL during liquidation,
and the rating takes into account the insurance fund balance, position
concentration on the symbol, order book depth, price volatility, average
leverage, unrealized PnL, and margin utilization at the symbol level.

The rating can be high, medium and low, and is updated every 30 minutes.

Operation ID: `adlRisk`

##### `GET /futures/data/basis`

Basis

Query future basis

Operation ID: `basis`

##### `GET /fapi/v1/time`

Check Server Time

Test connectivity to the Rest API and get the current server time.

Operation ID: `checkServerTime`

##### `GET /fapi/v1/indexInfo`

Composite Index Symbol Information

Query composite index symbol information

Operation ID: `compositeIndexSymbolInformation`

##### `GET /fapi/v1/aggTrades`

Compressed/Aggregate Trades List

Get compressed, aggregate market trades. Market trades that fill in
100ms with the same price and the same taking side will have the
quantity aggregated.

Retail Price Improvement(RPI) orders are aggregated and without special
tags to be distinguished.

Operation ID: `compressedAggregateTradesList`

##### `GET /fapi/v1/continuousKlines`

Continuous Contract Kline/Candlestick Data

Kline/candlestick bars for a specific contract type.
Klines are uniquely identified by their open time.

Operation ID: `continuousContractKlineCandlestickData`

##### `GET /fapi/v1/exchangeInfo`

Exchange Information

Current exchange trading rules and symbol information

Operation ID: `exchangeInformation`

##### `GET /fapi/v1/fundingRate`

Get Funding Rate History

Operation ID: `getFundingRateHistory`

##### `GET /fapi/v1/fundingInfo`

Get Funding Rate Info

Query funding rate info for symbols that had FundingRateCap/FundingRateFloor / fundingIntervalHours adjustment

Operation ID: `getFundingRateInfo`

##### `GET /fapi/v1/indexPriceKlines`

Index Price Kline/Candlestick Data

Kline/candlestick bars for the index price of a pair.
Klines are uniquely identified by their open time.

Operation ID: `indexPriceKlineCandlestickData`

##### `GET /fapi/v1/klines`

Kline/Candlestick Data

Kline/candlestick bars for a symbol.
Klines are uniquely identified by their open time.

Operation ID: `klineCandlestickData`

##### `GET /futures/data/globalLongShortAccountRatio`

Long/Short Ratio

Query symbol Long/Short Ratio

Operation ID: `longShortRatio`

##### `GET /fapi/v1/premiumIndex`

Mark Price

Mark Price and Funding Rate

Operation ID: `markPrice`

##### `GET /fapi/v1/markPriceKlines`

Mark Price Kline/Candlestick Data

Kline/candlestick bars for the mark price of a symbol.
Klines are uniquely identified by their open time.

Operation ID: `markPriceKlineCandlestickData`

##### `GET /fapi/v1/assetIndex`

Multi-Assets Mode Asset Index

Asset index price.

> **CM-UM Integration (Effective 2026-06-30):** Renamed from *Multi-Assets Mode Asset Index*. The response now additionally pushes COIN-M settlement-asset price index entries (e.g., `BTCUSD`, `ETHUSD`, `BNBUSD`). The endpoint path `/fapi/v1/assetIndex` is unchanged.

Operation ID: `assetIndex`

##### `GET /fapi/v1/historicalTrades`

Old Trades Lookup (MARKET_DATA)

Get older market historical trades.

Operation ID: `oldTradesLookup`

##### `GET /fapi/v1/openInterest`

Open Interest

Get present open interest of a specific symbol.

Operation ID: `openInterest`

##### `GET /futures/data/openInterestHist`

Open Interest Statistics

Operation ID: `openInterestStatistics`

##### `GET /fapi/v1/depth`

Order Book

Query symbol orderbook

Retail Price Improvement(RPI) orders are not visible and excluded in the
response message.

Operation ID: `orderBook`

##### `GET /fapi/v1/premiumIndexKlines`

Premium index Kline Data

Premium index kline bars of a symbol. Klines are uniquely identified by their open time.

Operation ID: `premiumIndexKlineData`

##### `GET /futures/data/delivery-price`

Quarterly Contract Settlement Price

Latest price for a symbol or symbols.

Operation ID: `quarterlyContractSettlementPrice`

##### `GET /fapi/v1/constituents`

Query Index Price Constituents

Query index price constituents

**Note**:
Prices from constituents of TradFi perps will be hiden and displayed as -1.

Operation ID: `queryIndexPriceConstituents`

##### `GET /fapi/v1/insuranceBalance`

Query Insurance Fund Balance Snapshot

Operation ID: `queryInsuranceFundBalanceSnapshot`

##### `GET /fapi/v1/trades`

Recent Trades List

Get recent market trades

Operation ID: `recentTradesList`

##### `GET /fapi/v1/rpiDepth`

RPI Order Book

Query symbol orderbook with RPI orders

RPI(Retail Price Improvement) orders are included and aggreated in the
response message. Crossed price levels are hidden and invisible.

Operation ID: `rpiOrderBook`

##### `GET /fapi/v1/ticker/bookTicker`

Symbol Order Book Ticker

Best price/qty on the order book for a symbol or symbols.

Retail Price Improvement(RPI) orders are not visible and excluded in the
response message.

Operation ID: `symbolOrderBookTicker`

##### `GET /fapi/v1/ticker/price`

Symbol Price Ticker

Latest price for a symbol or symbols.

Operation ID: `symbolPriceTicker`

##### `GET /fapi/v2/ticker/price`

Symbol Price Ticker V2

Latest price for a symbol or symbols.

Operation ID: `symbolPriceTickerV2`

##### `GET /futures/data/takerlongshortRatio`

Taker Buy/Sell Volume

Operation ID: `takerBuySellVolume`

##### `GET /fapi/v1/ping`

Test Connectivity

Test connectivity to the Rest API.

Operation ID: `testConnectivity`

##### `GET /fapi/v1/ticker/24hr`

24hr Ticker Price Change Statistics

24 hour rolling window price change statistics.
**Careful** when accessing this with no symbol.

Operation ID: `ticker24hrPriceChangeStatistics`

##### `GET /futures/data/topLongShortAccountRatio`

Top Trader Long/Short Account Ratio (MARKET_DATA)

The proportion of net long and net short accounts to total accounts of
the top 20% users with the highest margin balance. Each account is
counted once only.

Long Account % = Accounts of top traders with net long positions / Total
accounts of top traders with open positions

Short Account % = Accounts of top traders with net short positions /
Total accounts of top traders with open positions

Long/Short Ratio (Accounts) = Long Account % / Short Account %

Operation ID: `topTraderLongShortRatioAccounts`

##### `GET /futures/data/topLongShortPositionRatio`

Top Trader Long/Short Position Ratio (MARKET_DATA)

The proportion of net long and net short positions to total open
positions of the top 20% users with the highest margin balance.

Long Position % = Long positions of top traders / Total open positions
of top traders

Short Position % = Short positions of top traders / Total open positions
of top traders

Long/Short Ratio (Positions) = Long Position % / Short Position %

Operation ID: `topTraderLongShortRatioPositions`

##### `GET /fapi/v1/tradingSchedule`

Trading Schedule

Trading session schedules for the underlying assets of TradFi Perps are provided for a one-week period forward and one-week period backward starting from the day prior to the query time, covering the U.S. equity market, Korean equity market, Hong Kong equity market, China equity market, and the commodity market.

Session types per market:
- U.S. equity market: "PRE_MARKET", "REGULAR", "AFTER_MARKET", "OVERNIGHT", "NO_TRADING".
- Commodity market: "REGULAR", "NO_TRADING".
- Korean equity market: "REGULAR", "NO_TRADING".
- Hong Kong equity market: "REGULAR", "NO_TRADING".
- China equity market: "REGULAR", "NO_TRADING".

Operation ID: `tradingSchedule`


#### portfolio-margin-endpoints

##### `GET /fapi/v1/pmAccountInfo`

Classic Portfolio Margin Account Information (USER_DATA)

Get Classic Portfolio Margin current account information.

Operation ID: `classicPortfolioMarginAccountInformation`


#### user-data-streams

##### `PUT /fapi/v1/listenKey`

Keepalive User Data Stream (USER_STREAM)

Keepalive a user data stream to prevent a time out. User data streams
will close after 60 minutes. It's recommended to send a ping about every
60 minutes.

Operation ID: `keepaliveUserDataStream`

##### `POST /fapi/v1/listenKey`

Start User Data Stream (USER_STREAM)

Start a new user data stream. The stream will close after 60 minutes
unless a keepalive is sent. If the account has an active `listenKey`,
that `listenKey` will be returned and its validity will be extended for
60 minutes.

Operation ID: `startUserDataStream`

##### `DELETE /fapi/v1/listenKey`

Close User Data Stream (USER_STREAM)

Close out a user data stream.

Operation ID: `closeUserDataStream`


---

### Futures (USDⓈ-M) WebSocket API (1.0.0)

Access market data, manage accounts, and trade USDⓈ-M perpetual futures.


#### account

##### `POST /account.status`

Account Information (USER_DATA)

Get current account information. User in single-asset/ multi-assets mode will see different value, see comments in response section for detail.

Operation ID: `accountInformation`

##### `POST /v2/account.status`

Account Information V2 (USER_DATA)

Get current account information. User in single-asset/ multi-assets mode will see different value, see comments in response section for detail.

Operation ID: `accountInformationV2`

##### `POST /account.balance`

Futures Account Balance (USER_DATA)

Futures Account Balance

Operation ID: `futuresAccountBalance`

##### `POST /v2/account.balance`

Futures Account Balance V2 (USER_DATA)

Futures Account Balance V2

Operation ID: `futuresAccountBalanceV2`


#### market-data

##### `POST /depth`

Order Book

Get current order book. Note that this request returns limited market
depth.

If you need to continuously monitor order book updates, please consider
using Websocket Market Streams:
  * `<symbol>@depth<levels>`
  * `<symbol>@depth`

You can use `depth` request together with `<symbol>@depth` streams to
maintain a local order book.

**Note:**

- Retail Price Improvement(RPI) orders are not visible and excluded in
the response message.

Operation ID: `orderBook`

##### `POST /ticker.book`

Symbol Order Book Ticker

Best price/qty on the order book for a symbol or symbols.

**Note:**

- Retail Price Improvement(RPI) orders are not visible and excluded in
the response message.

Operation ID: `symbolOrderBookTicker`

##### `POST /ticker.price`

Symbol Price Ticker

Latest price for a symbol or symbols.

Operation ID: `symbolPriceTicker`


#### trade

##### `POST /algoOrder.cancel`

Cancel Algo Order (TRADE)

Cancel an active algo order.

Operation ID: `cancelAlgoOrder`

##### `POST /order.cancel`

Cancel Order (TRADE)

Cancel an active order.

Operation ID: `cancelOrder`

##### `POST /order.modify`

Modify Order (TRADE)

Order modify function, currently only LIMIT order modification is supported, modified orders will be reordered in the match queue

Operation ID: `modifyOrder`

##### `POST /algoOrder.place`

New Algo Order (TRADE)

Send in a new algo order.

Operation ID: `newAlgoOrder`

##### `POST /order.place`

New Order (TRADE)

Send in a new order.

Operation ID: `newOrder`

##### `POST /account.position`

Position Information (USER_DATA)

Get current position information.

Operation ID: `positionInformation`

##### `POST /v2/account.position`

Position Information V2 (USER_DATA)

Get current position information(only symbol that has position or open
orders will be returned).

Operation ID: `positionInformationV2`

##### `POST /order.status`

Query Order (USER_DATA)

Check an order's status.

* These orders will not be found:
  * order status is `CANCELED` or `EXPIRED` **AND** order has NO filled trade **AND** created time + 3 days < current time
  * order create time + 90 days < current time

Operation ID: `queryOrder`


#### user-data-streams

##### `POST /userDataStream.stop`

Close User Data Stream (USER_STREAM)

Close out a user data stream.

Operation ID: `closeUserDataStream`

##### `POST /userDataStream.ping`

Keepalive User Data Stream (USER_STREAM)

Keepalive a user data stream to prevent a time out. User data streams will close after 60 minutes. It's recommended to send a ping about every 60 minutes.

Operation ID: `keepaliveUserDataStream`

##### `POST /userDataStream.start`

Start User Data Stream (USER_STREAM)

Start a new user data stream. The stream will close after 60 minutes unless a keepalive is sent. If the account has an active `listenKey`, that `listenKey` will be returned and its validity will be extended for 60 minutes.

Operation ID: `startUserDataStream`


---

### Futures (USDⓈ-M) WebSocket Market Streams (1.0.0)

Access market data, manage accounts, and trade USDⓈ-M perpetual futures.


#### market

##### `POST /{symbol}@aggTrade`

Aggregate Trade Streams

The Aggregate Trade Streams push market trade information that is aggregated for fills with same price and taking side every 100 milliseconds. Only market trades will be aggregated, which means the insurance fund trades and ADL trades won't be aggregated.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM).

Operation ID: `aggregateTradeStreams`

##### `POST /!forceOrder@arr`

All Market Liquidation Order Streams

The All Liquidation Order Snapshot Streams push force liquidation order information for all symbols in the market. For each symbol，only the latest one liquidation order within 1000ms will be pushed as the snapshot. If no liquidation happens in the interval of 1000ms, no stream will be pushed.

> **After CM migration**, this stream pushes the merged UM + CM universe (subscribable on both `fstream` and `dstream`); each payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `allMarketLiquidationOrderStreams`

##### `POST /!miniTicker@arr`

All Market Mini Tickers Stream

24hr rolling window mini-ticker statistics for all symbols. These are NOT the statistics of the UTC day, but a 24hr rolling window from requestTime to 24hrs before. Note that only tickers that have changed will be present in the array.

> **After CM migration**, this stream pushes the merged UM + CM universe (subscribable on both `fstream` and `dstream`); each payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `allMarketMiniTickersStream`

##### `POST /!ticker@arr`

All Market Tickers Streams

24hr rolling window ticker statistics for all symbols. These are NOT the statistics of the UTC day, but a 24hr rolling window from requestTime to 24hrs before. Note that only tickers that have changed will be present in the array.

> **After CM migration**, this stream pushes the merged UM + CM universe (subscribable on both `fstream` and `dstream`); each payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `allMarketTickersStreams`

##### `POST /{symbol}@compositeIndex`

Composite Index Symbol Information Streams

Composite index information for index symbols pushed every second.

Operation ID: `compositeIndexSymbolInformationStreams`

##### `POST /{pair}_{contractType}@continuousKline_{interval}`

Continuous Contract Kline/Candlestick Streams

Continuous Contract Kline/Candlestick Streams

> **After CM migration**, both `fstream` and `dstream` may subscribe to either UM or CM symbols on this stream.

Operation ID: `continuousContractKlineCandlestickStreams`

##### `POST /!contractInfo`

Contract Info Stream

ContractInfo stream pushes when contract info updates(listing/settlement/contract bracket update). bks field only shows up when bracket gets updated.

> **After CM migration**, this stream pushes the merged UM + CM universe (subscribable on both `fstream` and `dstream`); each payload is appended with a new `st` field (`1` = UM, `2` = CM).

Operation ID: `contractInfoStream`

##### `POST /{symbol}@miniTicker`

Individual Symbol Mini Ticker Stream

24hr rolling window mini-ticker statistics for a single symbol. These are NOT the statistics of the UTC day, but a 24hr rolling window from requestTime to 24hrs before.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `individualSymbolMiniTickerStream`

##### `POST /{symbol}@ticker`

Individual Symbol Ticker Streams

24hr rolling window ticker statistics for a single symbol. These are NOT the statistics of the UTC day, but a 24hr rolling window from requestTime to 24hrs before.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `individualSymbolTickerStreams`

##### `POST /{symbol}@kline_{interval}`

Kline/Candlestick Streams

The Kline/Candlestick Stream push updates to the current klines/candlestick every 250 milliseconds (if existing).

> **After CM migration**, both `fstream` and `dstream` may subscribe to either UM or CM symbols on this stream.

Operation ID: `klineCandlestickStreams`

##### `POST /{symbol}@forceOrder`

Liquidation Order Streams

The Liquidation Order Snapshot Streams push force liquidation order information for specific symbol. For each symbol，only the latest one liquidation order within 1000ms will be pushed as the snapshot. If no liquidation happens in the interval of 1000ms, no stream will be pushed.

Operation ID: `liquidationOrderStreams`

##### `POST /{symbol}@markPrice@{updateSpeed}`

Mark Price Stream

Mark price and funding rate for a single symbol pushed every 3 seconds or every second.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM); both `fstream` and `dstream` may subscribe to either UM or CM symbols on this stream.

Operation ID: `markPriceStream`

##### `POST /!markPrice@arr@{updateSpeed}`

Mark Price Stream for All market

Mark price and funding rate for all symbols pushed every 3 seconds or every second.

**Note:**
- TradFi symbols will be pushed through a seperate message.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM); both `fstream` and `dstream` may subscribe to either UM or CM symbols on this stream.

Operation ID: `markPriceStreamForAllMarket`

##### `POST /!assetIndex@arr`

Multi-Assets Mode Asset Index

Asset index price. Subscribe with `!assetIndex@arr` for all assets, or `<assetSymbol>@assetIndex` for a specific asset.

> **CM-UM Integration (Effective 2026-06-30):** Renamed from *Multi-Assets Mode Asset Index*. The stream `!assetIndex@arr` now additionally pushes COIN-M settlement-asset price index entries (e.g., `BTCUSD`, `ETHUSD`, `BNBUSD`). The on-the-wire stream key is unchanged; existing subscriptions continue to work.

Operation ID: `assetIndex`

##### `POST /tradingSession`

Trading Session Stream

Trading session information for the underlying assets of TradFi Perpetual contracts, covering the U.S. equity market, Korean equity market, Hong Kong equity market, China equity market, and the commodity market, is updated every second. Trading session information for different underlying markets is pushed in separate messages.

**Event type:**

- `EquityUpdate`: Session types for the U.S. equity market include "PRE_MARKET", "REGULAR", "AFTER_MARKET", "OVERNIGHT", and "NO_TRADING".
- `CommodityUpdate`: Session types for the commodity market include "REGULAR" and "NO_TRADING".
- `KR_EquityUpdate`: Session types for the Korean equity market include "REGULAR" and "NO_TRADING".
- `HK_EquityUpdate`: Session types for the Hong Kong equity market include "REGULAR" and "NO_TRADING".
- `CN_EquityUpdate`: Session types for the China equity market include "REGULAR" and "NO_TRADING".

Operation ID: `tradingSessionStream`


#### public

##### `POST /!bookTicker`

All Book Tickers Stream

Pushes any update to the best bid or ask's price or quantity in real-time for all symbols.

> **After CM migration**, this stream pushes the merged UM + CM universe (subscribable on both `fstream` and `dstream`); each payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `allBookTickersStream`

##### `POST /{symbol}@depth@{updateSpeed}`

Diff. Book Depth Streams

Bids and asks, pushed every 250 milliseconds, 500 milliseconds, 100 milliseconds (if existing).

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `diffBookDepthStreams`

##### `POST /{symbol}@bookTicker`

Individual Symbol Book Ticker Streams

Pushes any update to the best bid or ask's price or quantity in real-time for a specified symbol.

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM).

Operation ID: `individualSymbolBookTickerStreams`

##### `POST /{symbol}@depth{levels}@{updateSpeed}`

Partial Book Depth Streams

Top <levels> bids and asks

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `partialBookDepthStreams`

##### `POST /{symbol}@rpiDepth@500ms`

RPI Diff. Book Depth Streams

Bids and asks including RPI orders, pushed every 500 milliseconds

> **After CM migration**, the payload is appended with a new `st` field (`1` = UM, `2` = CM) and a new `ps` field (pair symbol).

Operation ID: `rpiDiffBookDepthStreams`


---

