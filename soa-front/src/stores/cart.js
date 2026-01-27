import { defineStore } from 'pinia'
import api from '@/services/api'

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
        // Backend vraća { items: [...], total: 100 }
        this.items = res.data.items || []
        this.total = res.data.total || 0
      } catch (error) {
        console.error("Failed to load cart", error)
      }
    },

    // Dodaj u korpu
    async addToCart(tour) {
      try {
        // Backend očekuje AddCartDto: { TourId, Name, Price }
        await api.post('/cart', {
            tourId: tour.id,
            name: tour.name,
            price: tour.price
        })
        await this.loadCart() // Osveži stanje
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
        return res.data // Vraća tokene kupljenih tura
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})