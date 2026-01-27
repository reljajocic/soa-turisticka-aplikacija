<template>
  <div class="container">

    <div v-if="loading" class="loading-state">
        <i class="fa fa-spinner fa-spin"></i> Loading cart...
    </div>

    <div v-else-if="cartStore.items.length === 0" class="empty-cart card">
      <div class="empty-icon">
        <i class="fa fa-shopping-cart"></i>
      </div>
      <h3>Your cart is empty</h3>
      <p>Looks like you haven't added any tours yet.</p>
      <button @click="$router.push('/tours')" class="btn btn-outline-primary">
        <i class="fa fa-search"></i> Explore Tours
      </button>
    </div>

    <div v-else class="cart-container card">
      
      <div class="cart-items-list">
        <div v-for="item in cartStore.items" :key="item.tourId" class="cart-item">
          <div class="item-info">
             <span class="item-name">{{ item.name }}</span>
          </div>
          
          <div class="item-actions">
              <span class="item-price">${{ item.price.toFixed(2) }}</span>
              
              <button @click="removeItem(item.tourId)" class="btn-remove" title="Remove">
                  <i class="fa fa-trash"></i>
              </button>
          </div>
        </div>
      </div>

      <div class="cart-summary">
        <div class="summary-row total-row">
            <span>Total:</span>
            <span class="total-amount">${{ cartStore.total.toFixed(2) }}</span>
        </div>

        <button @click="handleCheckout" class="btn btn-checkout-lg">
          <i class="fa fa-credit-card-alt" aria-hidden="true"></i> Checkout
        </button>
        
        <p class="checkout-note">
            By clicking Checkout, you confirm your purchase.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useCartStore } from '@/stores/cart'

const cartStore = useCartStore()
const loading = ref(true)

onMounted(async () => {
    await cartStore.loadCart()
    loading.value = false
})

// 👇 NOVA FUNKCIJA
const removeItem = async (tourId) => {
    if(!confirm("Remove this tour from cart?")) return
    try {
        await cartStore.removeFromCart(tourId) // Moraš imati ovo u store-u!
    } catch (e) {
        alert("Failed to remove item.")
    }
}

const handleCheckout = async () => {
    if(!confirm("Are you sure you want to complete the purchase?")) return
    try {
        await cartStore.checkout()
        alert("Purchase successful! Get ready for your adventure!")
    } catch (e) {
        alert("Checkout failed: " + (e.response?.data?.message || e.message))
    }
}
</script>

<style scoped>
.page-title { text-align: center; margin: 30px 0; color: #2c3e50; font-size: 2rem; }
.loading-state { text-align: center; font-size: 1.2rem; color: #666; margin-top: 50px; }

.card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.05);
    border: none;
    overflow: hidden;
    max-width: 800px;
    margin: 0 auto;
}

.empty-cart { padding: 50px 20px; text-align: center; }
.empty-icon { font-size: 4rem; color: #ddd; margin-bottom: 20px; }
.empty-cart h3 { color: #2c3e50; margin-bottom: 10px; }
.empty-cart p { color: #777; margin-bottom: 30px; }

.cart-items-list { padding: 20px 30px; }

.cart-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 15px 0;
    border-bottom: 1px solid #eee;
}
.cart-item:last-child { border-bottom: none; }

.item-name { font-weight: 600; font-size: 1.1rem; color: #2c3e50; }

/* 👇 NOVI STILOVI ZA DESNU STRANU (Cena + Kanta) */
.item-actions {
    display: flex;
    align-items: center;
    gap: 15px;
}

.item-price { font-weight: 700; font-size: 1.1rem; color: #2c3e50; }

/* Dugme za kantu */
.btn-remove {
    background: #fee2e2;
    color: #ef4444;
    border: none;
    width: 35px;
    height: 35px;
    border-radius: 50%;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: 0.2s;
}
.btn-remove:hover {
    background: #fecaca;
    transform: scale(1.1);
}

.cart-summary {
    background: #f9f9f9;
    padding: 30px;
    border-top: 1px solid #eee;
}

.summary-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.total-row { font-size: 1.3rem; font-weight: bold; color: #2c3e50; }
.total-amount { color: #cc072a; font-size: 1.5rem; }

.checkout-note { text-align: center; font-size: 0.85rem; color: #888; margin-top: 15px; }

.btn {
    padding: 10px 20px;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border: none;
}

.btn-outline-primary {
    background: white;
    border: 2px solid #cc072a;
    color: #cc072a;
}
.btn-outline-primary:hover { background: #cc072a; color: white; }

.btn-checkout-lg {
    width: 100%;
    padding: 15px;
    background: #cc072a;
    color: white;
    font-size: 1.2rem;
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}
.btn-checkout-lg:hover {
    background: #99051f;
    transform: translateY(-2px);
    box-shadow: 0 6px 8px rgba(0,0,0,0.15);
}
</style>