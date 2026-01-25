import { defineStore } from 'pinia'
import api from '@/services/api'

export const useProfileStore = defineStore('profile', {
  state: () => ({
    profile: null
  }),

  actions: {
    async getMyProfile() {
      try {
        const response = await api.get('/profile/me')
        this.profile = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async updateProfile(profileData) {
      try {
        const response = await api.put('/profile/me', profileData)
        this.profile = response.data // Ažuriramo lokalno stanje
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})