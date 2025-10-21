import type UserModel from '@/models/auth/userModel'
import { defineStore } from 'pinia'

export const useUserStore = defineStore('user', {
  state: () => ({
    user: {} as UserModel
  }),
  actions: {
  }
})
