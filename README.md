# Smart Shop

ASP.NET Core Web API (.NET 10) + Razor Pages + EF Core (Code First) + SQL Server.
Gemini model `gemini-2.5-flash`, called through the REST API with `HttpClient`.

## Run

```bash
docker compose up -d
cd SmartShop
dotnet user-secrets set "Gemini:ApiKey" "<api-key>"
dotnet run
```

Website: http://localhost:5080

Database script: `Database/SMARTSHOP.sql`. Postman collection: `postman/SmartShop.postman_collection.json`.

## APIs

| Name | Route | Body |
|---|---|---|
| generatedescription | `POST /api/products/{id}/generatedescription` | |
| analyzereview | `POST /api/reviews/analyze` | `productId, customerName, content, rating` |
| shoppingassistant | `POST /api/assistant/chat` | `sessionId, message` |
| getreviewreport | `GET /api/reports/reviews` | |

Other: `GET /api/products`, `POST /api/products`, `GET/DELETE /api/assistant/history/{sessionId}`.

## Prompts

Source: `SmartShop/Services/Prompts.cs`.

### generatedescription

System instruction:

```text
You are a professional e-commerce copywriter for Smart Shop, an online electronics store.
Write persuasive, accurate product descriptions for customers.
Rules:
- Use ONLY the information provided (name, category, features). Never invent specifications, prices or promotions.
- Write at most 80 words, in one paragraph, in English.
- Highlight the customer benefit of each feature with a friendly, professional tone.
- Return only the description text: no title, no markdown, no bullet points, no quotes.
```

User prompt:

```text
Write a product description (maximum 80 words) for this product:
Product name: {ProductName}
Category: {Category}
Features: {Features}
```

### analyzereview

System instruction:

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

User prompt:

```text
Analyze this customer review.
Product: {ProductName} ({Category})
Customer: {customerName}
Rating: {rating}/5 stars
Review: "{content}"
```

Generation config (JSON mode + response schema):

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

### shoppingassistant

System instruction (`{catalog}` is built from the `Product` table on each request):

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
{catalog}
```

Catalog line format:

```text
- Id: 1 | Name: Galaxy S25 | Category: Phone | Price: 22.990.000 VND | Features: 6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera
```

User prompt: the `message` from the request. The history of the same `sessionId` (kept in `IMemoryCache`, 30 minutes, last 20 messages) is sent before it in `contents`.

## Gemini API key

- `appsettings.json` has `"ApiKey": ""`. The key is read from configuration `Gemini:ApiKey`.
- Development: User Secrets (`UserSecretsId` in `SmartShop.csproj`), stored in `%APPDATA%\Microsoft\UserSecrets\`, outside the project.
- Server: environment variable `Gemini__ApiKey`.
- The key is sent in the `x-goog-api-key` header, not in the URL.
