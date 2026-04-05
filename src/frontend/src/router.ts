import { createRouter, createWebHistory } from 'vue-router'
import Home from './components/Home.vue'
import RepositoryDetail from './components/RepositoryDetail.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/repository/:id',
    name: 'RepositoryDetail',
    component: RepositoryDetail,
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
