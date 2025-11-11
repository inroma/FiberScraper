import FiberMapVue from "@/views/fibers/FiberMap.vue";
import { type RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import AutoRefreshView from "@/views/autorefresh/AutoRefreshView.vue";
import Callback from "@/views/auth/Callback.vue";
import Login from "@/views/auth/Login.vue";
import Account from "@/views/account/Account.vue";
import { useUserStore } from "@/store/userStore";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "fibermap",
    component: FiberMapVue,
  },
  {
    path: "/auto-refresh",
    name: "autorefresh",
    component: AutoRefreshView,
    meta: { requireAuth: true },
  },
  {
    path: "/account",
    name: "account",
    component: Account,
    meta: { requireAuth: true }
  },
  {
    path: '/auth',
    redirect: '/',
    children: [
      {
        path: "callback",
        name: "authCallback",
        component: Callback,
      },
      {
        path: "login",
        name: "login",
        component: Login,
        meta: { allowAnonymous: true },
      }
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, _, next) => {
  if (to.meta?.allowAnonymous) {
    return next();
  }
  const userStore = useUserStore();
  if (to.meta?.requireAuth && !userStore.isConnected) {
    return next({ name: 'login' });
  }
  return next();
});

export default router;
