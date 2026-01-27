import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import ProfileView from '../views/ProfileView.vue'
import CreateTourView from '../views/CreateTourView.vue'
import ToursView from '../views/ToursView.vue'
import MyToursView from '../views/MyToursView.vue'
import CartView from '@/views/CartView.vue'
import TourDetailsView from '../views/TourDetailsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'home', component: HomeView },
    { path: '/login', name: 'login', component: LoginView },
    { path: '/register', name: 'register', component: RegisterView },
    { path: '/profile', name: 'profile', component: ProfileView, meta: {requestAuth: true}},
    { path: '/create-tour', name: 'create-tour', component: CreateTourView, meta: { requiresAuth: true }},
    { path: '/tours', name: 'tours', component: ToursView},
    { path: '/my-tours', name: 'my-tours', component: MyToursView, meta: { requiresAuth: true }},
    { path: '/cart', name: 'cart', component: CartView },
    { path: '/tour/:id', name: 'tour-details', component: TourDetailsView}, 
  ]
})

export default router