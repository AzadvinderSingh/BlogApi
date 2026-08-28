
export default {
  bootstrap: () => import('./main.server.mjs').then(m => m.default),
  inlineCriticalCss: true,
  baseHref: '/',
  locale: undefined,
  routes: [
  {
    "renderMode": 2,
    "route": "/"
  }
],
  entryPointToBrowserMapping: undefined,
  assets: {
    'index.csr.html': {size: 90977, hash: '94ff753e9444d50ddd5537b3c46914f5cb0a8c8d7c8e99764c3eed250682bf76', text: () => import('./assets-chunks/index_csr_html.mjs').then(m => m.default)},
    'index.server.html': {size: 65819, hash: '7555bf3ceddd4974695b4351bd1e65db664399fe95d895eaf6ae6b93141cc8d5', text: () => import('./assets-chunks/index_server_html.mjs').then(m => m.default)},
    'index.html': {size: 91070, hash: '2ee9a3b27e246cef4f7e75cfb0781d88ce3e036f4e5c3e2a4e0697a67fee0fe7', text: () => import('./assets-chunks/index_html.mjs').then(m => m.default)},
    'styles-27GV6IWR.css': {size: 230913, hash: 'JkXL5bx+Tl4', text: () => import('./assets-chunks/styles-27GV6IWR_css.mjs').then(m => m.default)}
  },
};
