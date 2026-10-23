using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Dtos;
using SmartShop.Services.Gemini;

namespace SmartShop.Services;

public class ShoppingAssistantService(SmartShopDbContext db, IGeminiClient gemini, ChatHistoryStore historyStore)
{
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct)
    {
        var products = await db.Products.AsNoTracking().OrderBy(p => p.ProductId).ToListAsync(ct);
        var systemInstruction = Prompts.ShoppingAssistantSystemInstruction(products);
        var sessionId = request.SessionId.Trim();
        var userMessage = new ChatMessage(ChatMessage.UserRole, request.Message.Trim());

        return await historyStore.WithSessionAsync(sessionId, async history =>
        {
            var conversation = history.Append(userMessage).ToList();
            var reply = await gemini.GenerateTextAsync(systemInstruction, conversation, temperature: 0.3, ct: ct);

            history.Add(userMessage);
            history.Add(new ChatMessage(ChatMessage.ModelRole, reply));

            var items = history.Select(m => new ChatHistoryItem(m.Role, m.Text)).ToList();
            return new ChatResponse(sessionId, reply, items);
        }, ct);
    }
}
