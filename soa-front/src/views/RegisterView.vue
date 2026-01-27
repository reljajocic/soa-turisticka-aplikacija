<template>
  <div class="auth-container">
    <div class="card auth-card">
      
      <div class="auth-header">
        <h2>Join Travela</h2>
        <p>Create an account to explore the world</p>
      </div>

      <div v-if="error" class="error-box">
        <i class="fa fa-exclamation-triangle"></i> {{ error }}
      </div>
      
      <form @submit.prevent="handleRegister" class="auth-form">
        
        <div class="form-group">
          <label>Username</label>
          <div class="input-wrapper">
            <i class="fa fa-user input-icon"></i>
            <input v-model="form.username" type="text" placeholder="Choose a username" required />
          </div>
        </div>

        <div class="form-group">
          <label>Email Address</label>
          <div class="input-wrapper">
            <i class="fa fa-envelope input-icon"></i>
            <input v-model="form.email" type="email" placeholder="name@example.com" required />
          </div>
        </div>

        <div class="form-group">
          <label>Password</label>
          <div class="input-wrapper">
            <i class="fa fa-lock input-icon"></i>
            <input v-model="form.password" type="password" placeholder="Create a strong password" required />
          </div>
        </div>

        <div class="form-group">
          <label>I want to be a...</label>
          <div class="input-wrapper">
            <i class="fa fa-compass input-icon"></i>
            <select v-model="form.role" required>
              <option value="0">Tourist (Explore & Buy Tours)</option>
              <option value="1">Guide/Author (Create Tours)</option>
            </select>
          </div>
        </div>

        <button type="submit" class="btn btn-primary btn-block">
          Create Account <i class="fa fa-user-plus"></i>
        </button>
      </form>

      <div class="auth-footer">
        <p>Already have an account? <router-link to="/login">Login here</router-link></p>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()
const error = ref('')
const form = ref({ username: '', email: '', password: '', role: 0 })

const handleRegister = async () => {
  try {
    await authStore.register(form.value)
    router.push('/login')
  } catch (err) {
    error.value = 'Registration failed. Username might be taken.'
    console.error(err)
  }
}
</script>

<style scoped>
/* Ponavljamo iste stilove radi konzistentnosti */
.auth-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 90vh; /* Malo više prostora za registraciju */
    padding: 20px;
}

.auth-card {
    width: 100%;
    max-width: 450px;
    padding: 40px;
    border-radius: 16px;
    box-shadow: 0 15px 35px rgba(0,0,0,0.1);
    background: white;
    border: none;
}

.auth-header { text-align: center; margin-bottom: 30px; }
.auth-header h2 { font-size: 2rem; color: #2c3e50; margin: 0 0 10px; font-weight: 800; }
.auth-header p { color: #888; font-size: 1rem; margin: 0; }

.form-group { margin-bottom: 20px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #444; font-size: 0.9rem; }

.input-wrapper { position: relative; }
.input-icon { position: absolute; left: 15px; top: 50%; transform: translateY(-50%); color: #999; font-size: 1.1rem;}

/* Stilovi za input i select */
.form-group input, .form-group select {
    width: 100%;
    padding: 14px 14px 14px 45px;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 1rem;
    transition: all 0.3s;
    background-color: #f9f9f9;
    /* Reset za select strelicu da izgleda lepše (opciono) */
    appearance: none; 
    -webkit-appearance: none;
}

/* Dodajemo našu strelicu za select */
.form-group select {
    background-image: url("data:image/svg+xml;charset=US-ASCII,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%22292.4%22%20height%3D%22292.4%22%3E%3Cpath%20fill%3D%22%23999%22%20d%3D%22M287%2069.4a17.6%2017.6%200%200%200-13-5.4H18.4c-5%200-9.3%201.8-12.9%205.4A17.6%2017.6%200%200%200%200%2082.2c0%205%201.8%209.3%205.4%2012.9l128%20127.9c3.6%203.6%207.8%205.4%2012.8%205.4s9.2-1.8%2012.8-5.4L287%2095c3.5-3.5%205.4-7.8%205.4-12.8%200-5-1.9-9.2-5.5-12.8z%22%2F%3E%3C%2Fsvg%3E");
    background-repeat: no-repeat;
    background-position: right 15px top 50%;
    background-size: 12px auto;
}

.form-group input:focus, .form-group select:focus {
    border-color: #cc072a;
    background-color: white;
    box-shadow: 0 0 0 4px rgba(204, 7, 42, 0.1);
    outline: none;
}

.btn-block { width: 100%; padding: 14px; font-size: 1.1rem; border-radius: 8px; display: flex; justify-content: center; align-items: center; gap: 10px; transition: 0.2s; }
.btn-block:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(204, 7, 42, 0.3); }

.error-box { background: #fee2e2; color: #b91c1c; padding: 12px; border-radius: 8px; margin-bottom: 20px; text-align: center; }

.auth-footer { text-align: center; margin-top: 25px; font-size: 0.95rem; color: #666; }
.auth-footer a { color: #cc072a; font-weight: bold; text-decoration: none; }
.auth-footer a:hover { text-decoration: underline; }
</style>