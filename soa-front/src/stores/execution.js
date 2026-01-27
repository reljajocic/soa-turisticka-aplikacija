import { defineStore } from 'pinia'
import api from '@/services/api'

export const useExecutionStore = defineStore('execution', {
  state: () => ({
    activeSessionId: null,
    loading: false
  }),

  actions: {
    async startTour(tourId) {
      // Backend očekuje StartExecDto: { TourId }
      const res = await api.post('/execution/start', { tourId })
      this.activeSessionId = res.data.id
      return res.data.id
    },

    async pollPosition(executionId) {
      // Backend očekuje PollDto: { ExecutionId }
      const res = await api.post('/execution/poll', { executionId })
      return res.data // { reached, progress, distanceKm, ... }
    },

    async finishTour(executionId, success) {
      // Backend očekuje FinishDto: { ExecutionId, Success }
      await api.post('/execution/finish', { executionId, success })
      this.activeSessionId = null
    },

    async getPurchasedTours() {
      try {
        const response = await api.get('/tours/purchased')
        // Nećemo pregaziti this.tours jer možda želimo da ih držimo odvojeno, 
        // ali za MyToursView možemo koristiti lokalnu varijablu.
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})