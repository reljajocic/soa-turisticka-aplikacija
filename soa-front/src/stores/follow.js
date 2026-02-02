import { defineStore } from 'pinia'
import api from '@/services/api'

export const useFollowStore = defineStore('follow', {
  actions: {
    async followUser(userId) {
      try {
        await api.post(`/follow/${userId}`)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async unfollowUser(userId) {
      try {
        await api.delete(`/follow/${userId}`)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async isFollowing(authorId) {
      try {
        const res = await api.get(`/follow/is-following?authorId=${authorId}`)
        return res.data.isFollowing
      } catch (error) {
        console.error("Check follow status failed", error)
        return false
      }
    }
  }
})