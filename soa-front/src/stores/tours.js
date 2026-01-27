import { defineStore } from 'pinia'
import api from '@/services/api'

export const useTourStore = defineStore('tours', {
  state: () => ({
    tours: [],
    currentTour: null
  }),

  actions: {
    // 1. Kreiranje same ture
    async createTour(tourData) {
      try {
        const response = await api.post('/tours', tourData)
        return response.data // Vraća ID ture
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // 2. Dodavanje ključne tačke
    async addKeypoint(tourId, keypointData) {
      try {
        await api.post(`/tours/${tourId}/keypoints`, keypointData)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // 3. Dobavljanje SVIH tura (za Explore Tours)
    async getAllTours() {
      try {
        const response = await api.get('/tours')
        this.tours = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // 4. Dobavljanje MOJIH tura (za My Tours - AUTOR)
    async getToursByAuthor(authorId) {
      try {
        const response = await api.get(`/tours/author/${authorId}`)
        this.tours = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },
    
    // 5. Dobavljanje KUPLJENIH tura (za My Tours - TURISTA) - NOVO
    async getPurchasedTours() {
      try {
        const response = await api.get('/tours/purchased')
        // Možemo koristiti ovo za prikaz
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // 6. Promena statusa (Draft/Publish/Archive)
    async updateStatus(tourId, status) {
        try {
          await api.post(`/tours/${tourId}/status?status=${status}`)
        } catch (error) {
          throw error.response?.data || error.message
        }
    },

    // 7. Dohvati jednu turu (sa tačkama)
    async getTour(id) {
      try {
        const response = await api.get(`/tours/${id}`)
        this.currentTour = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})