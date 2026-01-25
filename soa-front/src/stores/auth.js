import { defineStore } from 'pinia'
import api from '@/services/api'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  // Prilikom učitavanja, odmah probaj da izvučeš korisnika iz sačuvanog tokena
  const user = ref(getUserFromToken(token.value))

  const isAuthenticated = computed(() => !!token.value)

  // Pomocna funkcija koja cita podatke iz JWT tokena
  function getUserFromToken(tokenStr) {
    if (!tokenStr) return null
    try {
      // Dekodiranje Payload-a tokena (srednji deo tokena)
      const payload = JSON.parse(atob(tokenStr.split('.')[1]))
      
      // Backend obicno salje username u polju "unique_name" ili "sub" ili "username"
      // Prilagodi ovo onome sto vidis u tokenu, ali ovo pokriva vecinu slucajeva:
      return {
        id: payload.sub || payload.id,
        username: payload.unique_name || payload.username || payload.sub, 
        role: payload.role // Ako backend salje rolu u tokenu
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

  return { user, token, isAuthenticated, isGuide, isTourist, register, login, logout }
})