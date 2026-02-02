import { defineStore } from 'pinia'
import api from '@/services/api'

export const useExecutionStore = defineStore('execution', {
  state: () => ({
    activeSessionId: null,
    loading: false
  }),

  actions: {
    // Pokretanje ture
    async startTour(tourId) {
      try {
        const res = await api.post('/execution/start', { tourId })
        this.activeSessionId = res.data.id
        return res.data.id
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // Periodično proveravanje pozicije (Tačka 17)
    async pollPosition(executionId) {
      try {
        // Šaljemo PollDto: { ExecutionId: "..." }
        const res = await api.post('/execution/poll', { executionId })
        return res.data 
      } catch (error) {
        console.error("Store polling error:", error)
        throw error
      }
    },

    // Završavanje ture (Success/Abandoned)
    async finishTour(executionId, success) {
      try {
        await api.post('/execution/finish', { executionId, success })
        this.activeSessionId = null
      } catch (error) {
        console.error("Store finish error:", error)
      }
    },

    // Pomoćna funkcija za proveru kupljenih tura
    async getPurchasedTours() {
      try {
        const response = await api.get('/tours/purchased')
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})