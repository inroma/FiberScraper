// Utilities
import { Map, View } from 'ol';
import { defineStore } from 'pinia'

export const useMapStore = defineStore('map', {
  state: () => ({
    location: [ 4.832225765809024, 45.75787095172237 ],
    zoom: 12,
    maxBounds: [ -7, 41, 12, 52 ],
    view: {} as View,
    map: {} as Map
  }),
  actions: {
  },
  persist: {
    storage: sessionStorage,
    omit: ['map', 'view', 'maxBounds']
  }
})
