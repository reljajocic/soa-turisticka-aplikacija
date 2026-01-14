<template>
  <div class="container">
    <div class="card" style="max-width: 400px; margin: 50px auto;">
      <h2>Login</h2>
      <div v-if="error" class="alert-error">{{ error }}</div>
      
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Username</label>
          <input v-model="username" type="text" required />
        </div>
        <div class="form-group">
          <label>Password</label>
          <input v-model="password" type="password" required />
        </div>
        <button type="submit" class="btn btn-primary">Login</button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')

const handleLogin = async () => {
  try {
    await authStore.login({ username: username.value, password: password.value })
    router.push('/')
  } catch (err) {
    error.value = 'Invalid credentials'
  }
}
</script>