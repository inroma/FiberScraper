<template>
  <v-app>
    <v-app-bar app hide-on-scroll clipped-left scroll-threshold="80">
      <v-app-bar-nav-icon v-if="smallScreen" @click.stop="drawer = true"/>
      <v-toolbar-title>Fiber Evolution Scraper</v-toolbar-title>
    </v-app-bar>
    <v-navigation-drawer v-model="drawer" app expand-on-hover clipped class="pa-0 nav-bar" mini-variant>
      <v-list>
        <v-list-item v-for="header in headers" link :to="header.url" :key="'header'+header.title">
          <v-list-item-icon><v-icon>{{ header.icon }}</v-icon></v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title class="text-h6">{{ header.title }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list>
      <template v-slot:append>
        <v-list-item class="pl-10 pr-10 pb-3">
          <v-list-item-content>
            <v-btn @click="$vuetify.theme.dark = !$vuetify.theme.dark" color="primary">
              <v-icon>mdi-theme-light-dark</v-icon>
              Theme Switch
            </v-btn>
          </v-list-item-content>
        </v-list-item>
      </template>
    </v-navigation-drawer>
    <v-main>
      <toast/>
      <router-view key="router-view" class="ma-5"/>
    </v-main>
  </v-app>
</template>

<script lang="ts" src="./App.ts" />

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

.nav-bar {
  z-index: 1000 !important;
}

nav {
  padding: 30px;
}

nav a {
  font-weight: bold;
  color: #2c3e50;
}

nav a.router-link-exact-active {
  color: #42b983;
}
</style>