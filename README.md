# Smart Shop – Generative AI with Google Gemini (SET01)

ASP.NET Core Web API (.NET 10, Controllers) + Razor Pages + EF Core Code First + SQL Server (Docker).
Gemini is called through its **REST API** with a typed `HttpClient`, model **`gemini-2.5-flash`**.

## Project structure

```
googleAI/
├─ README.md                      ← this file (requirement 6)
├─ docker-compose.yml             ← SQL Server 2022 container
├─ Database/SMARTSHOP.sql         ← database script (tables + initial data + report query)
├─ postman/SmartShop.postman_collection.json
└─ SmartShop/                     ← ASP.NET Core project
   ├─ Controllers/                ← ProductsController, ReviewsController, AssistantController, ReportsController
   ├─ Services/                   ← business logic + Prompts.cs (all prompts in one place)
   │  └─ Gemini/                  ← GeminiClient (REST), options, exceptions, DTOs
   ├─ Infrastructure/             ← GlobalExceptionHandler (429, invalid JSON, ...)
   ├─ Data/ Models/ Migrations/   ← EF Core Code First (database SMARTSHOP)
   ├─ Pages/                      ← Razor Pages website (Reports = requirement 5)
   └─ SmartShop.http              ← quick API tests
```

## How to run

```bash
# 1. Start SQL Server (port 14333)
docker compose up -d

# 2. Store the Gemini API key in User Secrets (NOT in source code)
cd SmartShop
dotnet user-secrets set "Gemini:ApiKey" "<your-gemini-api-key>"

# 3. Run – the SMARTSHOP database is created/migrated and seeded automatically
dotnet run
```

Open <http://localhost:5080/Reports>. Without Docker, change `ConnectionStrings:SmartShop` in
`appsettings.json` (or set `ConnectionStrings__SmartShop`) and/or run `Database/SMARTSHOP.sql`.

## APIs

| # | Name | Method & route | Body |
|---|---|---|---|
| 1 | generatedescription | `POST /api/products/{id}/generatedescription` | – |
| 2 | analyzereview | `POST /api/reviews/analyze` | `{ productId, customerName, content, rating }` |
| 3 | shoppingassistant | `POST /api/assistant/chat` | `{ sessionId, message }` |
| 4 | getreviewreport | `GET /api/reports/reviews` | – |

Other endpoints: `GET /api/products`, `POST /api/products` (add a product:
`{ productName, category, price, features }`), `GET/DELETE /api/assistant/history/{sessionId}`.

Example response of **getreviewreport**:

```json
[
  { "no": 1, "productName": "Galaxy S25",     "totalReviews": 5, "positive": 3, "negative": 1, "neutral": 1, "averageRating": 4.0 },
  { "no": 2, "productName": "MacBook Air M4", "totalReviews": 3, "positive": 3, "negative": 0, "neutral": 0, "averageRating": 4.7 },
  { "no": 3, "productName": "AirPods Pro 3",  "totalReviews": 2, "positive": 0, "negative": 2, "neutral": 0, "averageRating": 1.5 }
]
```

Website (requirement 5, Razor Pages):

- `/Reports` – the review report table (+ a form that calls analyzereview)
- `/` – products with a "Generate description" button (calls generatedescription)
- `/Assistant` – chat UI for the shopping assistant (keeps a sessionId)

---

## Prompts (requirement 6)

All prompts are in [`SmartShop/Services/Prompts.cs`](SmartShop/Services/Prompts.cs).

### Requirement 1 – generatedescription

**System instruction**

```text
You are a professional e-commerce copywriter for Smart Shop, an online electronics store.
Write persuasive, accurate product descriptions for customers.
Rules:
- Use ONLY the information provided (name, category, features). Never invent specifications, prices or promotions.
- Write at most 80 words, in one paragraph, in English.
- Highlight the customer benefit of each feature with a friendly, professional tone.
- Return only the description text: no title, no markdown, no bullet points, no quotes.
```

**User prompt** (filled from the `Product` row)

```text
Write a product description (maximum 80 words) for this product:
Product name: {ProductName}
Category: {Category}
Features: {Features}
```

Settings: temperature 0.8. The result is saved to `Product.Description`; as a safety net the
server also cuts the text to 80 words if the model ever writes more.

### Requirement 2 – analyzereview

**System instruction**

```text
You are a customer review analyst for Smart Shop, an online electronics store.
Analyze the sentiment of the customer review you receive.
Rules:
- sentiment must be exactly one of: "Positive", "Negative", "Neutral".
- score is your confidence in that sentiment, a number from 0.0 to 1.0.
- summary is one short English sentence (maximum 20 words) describing the main points of the review.
- Judge mainly from the review text; use the star rating only as a supporting signal.
- Respond with JSON only, following the given schema.
```

**User prompt**

```text
Analyze this customer review.
Product: {ProductName} ({Category})
Customer: {customerName}
Rating: {rating}/5 stars
Review: "{content}"
```

**Structured output** – `generationConfig` sent to Gemini (JSON mode + response schema):

```json
{
  "temperature": 0.2,
  "responseMimeType": "application/json",
  "responseSchema": {
    "type": "OBJECT",
    "properties": {
      "sentiment": { "type": "STRING", "enum": ["Positive", "Negative", "Neutral"] },
      "score":     { "type": "NUMBER", "description": "Confidence score from 0.0 to 1.0" },
      "summary":   { "type": "STRING", "description": "Short summary of the review" }
    },
    "required": ["sentiment", "score", "summary"],
    "propertyOrdering": ["sentiment", "score", "summary"]
  }
}
```

The JSON text is deserialized into the C# class `ReviewAnalysis { Sentiment, Score, Summary }`,
validated (sentiment in the enum, 0 ≤ score ≤ 1, summary not empty) and saved with the review
into the `Review` table.

### Requirement 3 – shoppingassistant

**System instruction** (the product catalog is loaded from the `Product` table on every request)

```text
You are the AI shopping assistant of Smart Shop, an online electronics store.
You ONLY answer questions about the products in the PRODUCT CATALOG below
(features, prices, comparisons, recommendations among these products).

Rules:
1. Use only the data in the PRODUCT CATALOG. Never invent products, prices, specifications, stock or promotions.
   If the catalog does not contain the information, say you do not have that information.
2. If the question is not related to Smart Shop products (e.g. weather, politics, coding, math, other stores),
   politely refuse with: "Sorry, I can only help with questions about Smart Shop products."
3. Use the previous messages of the conversation to understand follow-up questions
   (e.g. "How much does it cost?" refers to the product discussed before).
4. Prices are in VND. Answer briefly (at most 3 sentences), in the same language as the customer.

PRODUCT CATALOG:
- Id: 1 | Name: Galaxy S25 | Category: Phone | Price: 22.990.000 VND | Features: 6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera
- Id: 2 | Name: MacBook Air M4 | Category: Laptop | Price: 28.490.000 VND | Features: 13.6 inch, 16GB RAM, 18 hours battery
- Id: 3 | Name: AirPods Pro 3 | Category: Accessory | Price: 6.790.000 VND | Features: Active noise cancellation, USB-C, IP57
- Id: 4 | Name: Xiaomi Redmi Note 15 | Category: Phone | Price: 6.490.000 VND | Features: 120Hz display, 5500mAh battery, 108MP camera
```

**User prompt** = the customer's `message`, sent after the stored history of the same `sessionId`:

```json
"contents": [
  { "role": "user",  "parts": [{ "text": "Which phone has the biggest battery?" }] },
  { "role": "model", "parts": [{ "text": "The Xiaomi Redmi Note 15 has the biggest battery (5500mAh)." }] },
  { "role": "user",  "parts": [{ "text": "How much does it cost?" }] }
]
```

**Chat history:** `ChatHistoryStore` keeps the messages of each `sessionId` in `IMemoryCache`
(sliding expiration 30 minutes, last 20 messages). A turn is stored only after Gemini answers,
so a failed call never breaks the conversation. Settings: temperature 0.3.

---

## How the Gemini API key is protected

1. **Never in source code.** `appsettings.json` contains `"ApiKey": ""`; the real key is read at
   runtime from configuration key `Gemini:ApiKey` via `IOptions<GeminiOptions>`.
2. **Development – User Secrets.** The project has a `UserSecretsId`; the key is stored outside
   the project folder (`%APPDATA%\Microsoft\UserSecrets\smartshop-genai-set01\secrets.json`):
   ```bash
   dotnet user-secrets set "Gemini:ApiKey" "<your-key>"
   ```
3. **Server / CI – environment variable** (overrides other sources):
   ```bash
   # PowerShell
   $env:Gemini__ApiKey = "<your-key>"
   # bash
   export Gemini__ApiKey="<your-key>"
   ```
4. **Not leaked at runtime.** The key is sent in the `x-goog-api-key` HTTP header (not in the
   URL query string), so it never appears in URLs or request logs, and error responses never
   include it.
5. **Not submitted.** The key lives only in User Secrets (outside the project folder), so no
   submitted file contains it; `bin/` and `obj/` are removed before submission. If the key is missing the app still starts and the AI endpoints return a clear
   `503 Gemini is not configured` message.

> The SQL Server password in `docker-compose.yml` / `appsettings.json` is only for the local
> Docker development database.

## Error handling

`GlobalExceptionHandler` converts errors into RFC 7807 ProblemDetails JSON:

| Situation | HTTP status |
|---|---|
| Invalid body (rating not 1–5, empty content, ...) | 400 (model validation) |
| Product not found | 404 |
| Gemini quota exceeded (429) – retried with back-off first, `Retry-After` forwarded | 429 |
| Gemini returns invalid / empty / blocked JSON or text | 502 |
| Gemini unreachable or other Gemini HTTP error | 502 |
| API key not configured | 503 |
| Unexpected error (details are logged, not returned) | 500 |

## Submission checklist

- [x] Source code without `bin/` and `obj/`
- [x] Database script: `Database/SMARTSHOP.sql` (+ EF Core migration in `SmartShop/Migrations`)
- [x] README.md (this file)
- [x] Postman collection: `postman/SmartShop.postman_collection.json`
- [ ] Screenshots of each API with a real Gemini key (take them after setting your key)
