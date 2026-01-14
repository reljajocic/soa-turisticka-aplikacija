import { defineStore } from 'pinia'
import api from '@/services/api'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(JSON.parse(localStorage.getItem('user')) || null)
  const token = ref(localStorage.getItem('token') || null)

  const isAuthenticated = computed(() => !!token.value)

  function isGuide() {
    return user.value?.role === 'Author'
  }

  function isTourist() {
    return user.value?.role === 'Tourist'
  }

  async function register(userData) {
    try {
      // Konverzija Role (0 -> Tourist, 1 -> Author) za backend
      const payload = { 
        ...userData, 
        role: parseInt(userData.role) === 1 ? 'Author' : 'Tourist' 
      }
      
      const response = await api.post('/auth/register', payload)
      return response.data
    } catch (error) {
      throw error
    }
  }

  async function login(credentials) {
    try {
      const response = await api.post('/auth/login', credentials)
      const newToken = response.data.token
      
      token.value = newToken
      localStorage.setItem('token', newToken)
      
      // Ovde bi idealno odmah učitali profil korisnika, 
      // ali za sada ćemo samo setovati da je ulogovan.
      // user.value = { username: credentials.username } 
    } catch (error) {
      throw error
    }
  }

  function logout() {
    user.value = null
    token.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  return { user, token, isAuthenticated, isGuide, isTourist, register, login, logout }
})