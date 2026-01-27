import { defineStore } from 'pinia'
import api from '@/services/api' 

// NEMA VIŠE 'import axios ...' - To je pravilo problem!

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [],
    total: 0
  }),

  actions: {
    // Učitaj korpu
    async loadCart() {
      try {
        const res = await api.get('/cart')
        this.items = res.data.items || []
        this.total = res.data.total || 0
      } catch (error) {
        console.error("Failed to load cart", error)
      }
    },

    // Dodaj u korpu
    async addToCart(tour) {
      try {
        await api.post('/cart', {
            tourId: tour.id,
            name: tour.name,
            price: tour.price
        })
        await this.loadCart()
        alert('Added to cart!')
      } catch (error) {
        alert('Failed to add to cart: ' + (error.response?.data || error.message))
      }
    },

    // Kupi (Checkout)
    async checkout() {
      try {
        const res = await api.post('/cart/checkout')
        this.items = []
        this.total = 0
        return res.data 
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // BRISANJE (Koristimo 'api' umesto 'axios')
    async removeFromCart(tourId) {
          try {
              // Ovde sada koristimo 'api' koji je importovan gore
              // URL je '/cart/ID' jer 'api' sam zna gde je server
              await api.delete(`/cart/${tourId}`)
              
              // Ažuriramo lokalno stanje
              this.items = this.items.filter(i => i.tourId !== tourId)
              
              // Preračunaj total
              this.total = this.items.reduce((sum, item) => sum + item.price, 0)
              
          } catch (error) {
              console.error("Error removing item:", error)
              throw error
          }
      }
  }
})