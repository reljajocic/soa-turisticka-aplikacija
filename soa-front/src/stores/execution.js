import { defineStore } from 'pinia'
import api from '@/services/api'

export const useExecutionStore = defineStore('execution', {
  state: () => ({
    activeSessionId: null,
    executions: [], 
    loading: false
  }),

  actions: {
    async startTour(tourId) {
      try {
        const res = await api.post('/execution/start', { tourId })
        this.activeSessionId = res.data.id
        return res.data.id
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async pollPosition(executionId) {
      try {
        const res = await api.post('/execution/poll', { executionId })
        return res.data 
      } catch (error) {
        console.error("Store polling error:", error)
        throw error
      }
    },

    async finishTour(executionId, success) {
      try {
        await api.post('/execution/finish', { executionId, success })
        this.activeSessionId = null
      } catch (error) {
        console.error("Store finish error:", error)
      }
    },

    async getUserExecutions() {
      try {
        const res = await api.get('/execution/user')
        this.executions = res.data
        return res.data
      } catch (error) {
        console.error("Error fetching executions:", error)
        throw error
      }
    },

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