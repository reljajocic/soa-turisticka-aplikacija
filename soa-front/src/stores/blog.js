import { defineStore } from 'pinia'
import api from '@/services/api'

export const useBlogStore = defineStore('blog', {
  state: () => ({
    blogs: [],
    currentBlog: null,
    loading: false
  }),

  actions: {
    // 1. Dovuci sve blogove (Feed)
    async getAllBlogs() {
      this.loading = true
      try {
        const res = await api.get('/blog')
        this.blogs = res.data
      } catch (error) {
        console.error("Failed to fetch blogs", error)
        throw error
      } finally {
        this.loading = false
      }
    },

    // 2. Dovuci MOJE blogove
    async getMyBlogs() {
        this.loading = true
        try {
          const res = await api.get('/blog/my')
          this.blogs = res.data
        } catch (error) {
          console.error("Failed to fetch my blogs", error)
          throw error
        } finally {
          this.loading = false
        }
    },

    // 3. Kreiraj novi blog
    async createBlog(blogData) {
      try {
        await api.post('/blog', blogData)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    // 4. Dodaj komentar
    async addComment(blogId, content) {
        try {
            await api.post(`/blog/${blogId}/comments`, { content })
            // Opciono: Osveži trenutni blog da se vidi komentar
            // await this.getBlogById(blogId) 
        } catch (error) {
            throw error.response?.data || error.message
        }
    },

    async getBlog(id) {
        this.loading = true
        try {
            const res = await api.get(`/blog/${id}`)
            this.currentBlog = res.data
            return res.data
        } catch (error) {
            console.error(error)
            throw error
        } finally {
            this.loading = false
        }
    },
  }
})