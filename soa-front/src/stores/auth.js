import { defineStore } from 'pinia'
import api from '@/services/api'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const user = ref(getUserFromToken(token.value))

  const isAuthenticated = computed(() => !!token.value)

  function getUserFromToken(tokenStr) {
    if (!tokenStr) return null
    try {
      const payload = JSON.parse(atob(tokenStr.split('.')[1]))
      return {
        id: payload.sub || payload.id,
        username: payload.unique_name || payload.username || payload.sub || 'User', 
        role: payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
        
        avatarUrl: payload.avatarUrl || null 
      }
    } catch (e) {
      return null
    }
  }

  function isGuide() {
    return user.value?.role === 'Author'
  }

  function isTourist() {
    return user.value?.role === 'Tourist'
  }

  async function register(userData) {
    try {
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
      
      const userData = getUserFromToken(newToken)
      if (userData && !userData.username) {
          userData.username = credentials.username
      }
      user.value = userData
    } catch (error) {
      throw error
    }
  }

  function logout() {
    user.value = null
    token.value = null
    localStorage.removeItem('token')
  }

  function updateUser(userData) {
    if (user.value) {
        user.value = {
            ...user.value,
            username: userData.username || user.value.username,
            avatarUrl: userData.avatarUrl || user.value.avatarUrl
        }
    }
  }

  // OVO JE JEDINI RETURN KOJI TREBA DA POSTOJI
  return { 
      user, token, isAuthenticated, isGuide, isTourist, register, login, logout, 
      updateUser
  }
})