# 💰 Azure Service Bus Pricing Tiers - Quick Reference

## Which tier should you use?

### ✅ Use **Basic** if you:

- Only need **queues** (simple message queue)
- Have **one consumer** per queue
- Don't need advanced features
- Want to **save money** (lowest cost)

**👉 This is perfect for your application!** You only need a queue to send messages from App Service to Function App.

---

### ⬆️ Use **Standard** if you need:

- **Topics with subscriptions** (pub/sub pattern with multiple subscribers)
- **Transactions** (atomic operations)
- **Duplicate detection** (automatically filter duplicate messages)
- **Auto-forwarding** (chain queues/topics)
- **Dead-letter queues** (Basic has this too)
- **Scheduled messages** (send messages in the future)

---

### 🚀 Use **Premium** if you need:

- **Guaranteed performance** (dedicated resources)
- **Larger messages** (up to 100 MB vs 256 KB)
- **VNet integration** (private networking)
- **Geo-disaster recovery**
- **High throughput** (millions of messages)
- **Availability zones**

---

## Feature Comparison Table

| Feature                   | Basic  | Standard | Premium |
| ------------------------- | :----: | :------: | :-----: |
| **Queues**                |   ✅   |    ✅    |   ✅    |
| **Topics/Subscriptions**  |   ❌   |    ✅    |   ✅    |
| **Max message size**      | 256 KB |  256 KB  | 100 MB  |
| **Transactions**          |   ❌   |    ✅    |   ✅    |
| **Duplicate detection**   |   ❌   |    ✅    |   ✅    |
| **Scheduled messages**    |   ❌   |    ✅    |   ✅    |
| **Dead-letter queues**    |   ✅   |    ✅    |   ✅    |
| **Sessions**              |   ✅   |    ✅    |   ✅    |
| **Auto-forwarding**       |   ❌   |    ✅    |   ✅    |
| **Geo-disaster recovery** |   ❌   |    ❌    |   ✅    |
| **VNet integration**      |   ❌   |    ❌    |   ✅    |
| **Dedicated resources**   |   ❌   |    ❌    |   ✅    |
| **Availability zones**    |   ❌   |    ❌    |   ✅    |

---

## Pricing (Approximate - US East Region)

| Tier         | Base Cost    | Operations Cost              |
| ------------ | ------------ | ---------------------------- |
| **Basic**    | ~$0.05/month | $0.05 per million operations |
| **Standard** | ~$10/month   | $0.05 per million operations |
| **Premium**  | ~$677/month  | Included                     |

💡 **Note**: Prices are approximate and vary by region. Check [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/) for exact pricing.

---

## For Your Application

### Current Architecture:

```
Static Web App → App Service → Service Bus Queue → Function App
                                    ↑
                              Only 1 consumer
```

### Recommendation: **Basic Tier** ✅

**Why?**

- You only use a **queue** (not topics)
- You have **one consumer** (Function App)
- You don't need transactions or duplicate detection
- **Saves ~$10/month** compared to Standard

### When to upgrade to Standard:

If in the future you need:

1. **Multiple subscribers** to the same message (use topics)
2. **Guaranteed message ordering** with sessions
3. **Automatic duplicate filtering**

### When to upgrade to Premium:

If you scale to:

1. **High volume** (millions of messages per day)
2. **Large messages** (> 256 KB)
3. **Mission-critical** workloads requiring guaranteed performance
4. **Private networking** requirements (VNet)

---

## Migration Path

You can **upgrade at any time** without data loss:

1. **Basic → Standard**:

   - Go to Service Bus namespace in Azure Portal
   - Click "Pricing tier"
   - Select "Standard"
   - Click "Apply"
   - ✅ All queues and messages preserved

2. **Standard → Premium**:
   - Requires migration (cannot directly upgrade)
   - Create new Premium namespace
   - Migrate messages using tools

---

## Cost Optimization Tips

### If using Basic:

- ✅ Perfect for dev/test environments
- ✅ Good for low-to-medium volume production
- ✅ Can handle thousands of messages per day

### If using Standard:

- Enable **auto-delete on idle** for unused queues/topics
- Set appropriate **message TTL** (time-to-live)
- Monitor and remove unused subscriptions

### If using Premium:

- Use **messaging units** efficiently
- Consider **resource throttling** for cost control
- Use **metrics** to optimize capacity

---

## Quick Decision Tree

```
Do you need Topics with multiple subscribers?
│
├─ NO → Do you need Transactions or Duplicate Detection?
│       │
│       ├─ NO → ✅ Use Basic ($0.05/month)
│       │
│       └─ YES → Use Standard ($10/month)
│
└─ YES → Use Standard ($10/month)

Do you need guaranteed performance or >256KB messages?
│
└─ YES → Use Premium ($677/month)
```

---

## Summary for Your Project

✅ **Use Basic tier** - it's perfect for your needs!

Your application only requires:

- Simple queue (✅ Basic supports)
- Send messages from App Service (✅ Basic supports)
- Receive messages in Function App (✅ Basic supports)
- Dead-letter queue for failed messages (✅ Basic supports)

**You'll save money** and get all the features you need! 💰

---

## References

- [Service Bus Pricing](https://azure.microsoft.com/pricing/details/service-bus/)
- [Service Bus Tiers Documentation](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-premium-messaging)
- [Feature Comparison](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-messaging-overview)
