import {defineConfig4CustomTheme, UserPlugins} from 'vuepress/config'
import {VdoingThemeConfig} from 'vuepress-theme-vdoing/types'
import dayjs from 'dayjs'

const DOMAIN_NAME = 'xugaoyi.com' // 域名 (不带https)
const WEB_SITE = `https://${DOMAIN_NAME}` // 网址

export default defineConfig4CustomTheme<VdoingThemeConfig>({
    theme: 'vdoing', // 使用npm主题包

    locales: {
        '/': {
            lang: 'zh-CN',
            title: "ChatImage",
            description: '✨ 在Minecraft聊天栏中显示图片 ✨',
        }
    },
    // base: '/', // 默认'/'。如果你想将你的网站部署到如 https://foo.github.io/bar/，那么 base 应该被设置成 "/bar/",（否则页面将失去样式等文件）

    // 主题配置
    themeConfig: {
        // 导航配置
        nav: [
            {text: '首页', link: '/'},
            {
                text: '指南',
                link: '/wiki/chatimage/quick',
                items: [
                    {text: '快速开始🎈', link: '/wiki/chatimage/quick/'},
                    {text: 'ChatImageCode', link: '/wiki/chatimage/code/'},
                    {text: 'ChatImageStyle', link: '/wiki/chatimage/style/'},
                    {text: '常见错误🐛', link: '/wiki/chatimage/error/'},


                ]
            }
        ],
        sidebarDepth: 2, // 侧边栏显示深度，默认1，最大2（显示到h3标题）
        logo: '/img/logo.png', // 导航栏logo
        repo: 'kitUIN/ChatImage', // 导航栏右侧生成Github链接
        searchMaxSuggestions: 10, // 搜索结果显示最大数
        lastUpdated: '上次更新', // 开启更新时间，并配置前缀文字   string | boolean (取值为git提交时间)
        docsDir: 'docs', // 编辑的文件夹
        // docsBranch: 'master', // 编辑的文件所在分支，默认master。 注意：如果你的分支是main则修改为main
        editLinks: false, // 启用编辑
        editLinkText: '编辑',

        sidebar: 'structuring',

        // 文章默认的作者信息，(可在md文件中单独配置此信息) string | {name: string, link?: string}
        author: { // 文章默认的作者信息，可在md文件中单独配置此信息 String | {name: String, href: String}
            name: 'kitUIN', // 必需
            href: 'https://github.com/kitUIN' // 可选的
        },
        social: { // 社交图标，显示于博主信息栏和页脚栏
            // iconfontCssFile: '//at.alicdn.com/t/font_1678482_u4nrnp8xp6g.css', // 可选，阿里图标库在线css文件地址，对于主题没有的图标可自由添加
            icons: [{
                    iconClass: 'icon-youjian',
                    title: '发邮件',
                    link: 'mailto:kulujun@gmail.com',
                },
                {
                    iconClass: 'icon-bilibili',
                    title: 'BILIBILI',
                    link: 'https://space.bilibili.com/61924180',
                },
                {
                    iconClass: 'icon-github',
                    title: 'GitHub',
                    link: 'https://github.com/kitUIN',
                },
            ],
        },
        footer: {
            // 页脚信息
            createYear: 2021, // 博客创建年份
            copyrightInfo: 'kitUIN | <a href="https://github.com/xugaoyi/vuepress-theme-vdoing/blob/master/LICENSE" target="_blank">MIT License</a>', // 博客版权信息，支持a标签
        },

        // 扩展自动生成frontmatter。（当md文件的frontmatter不存在相应的字段时将自动添加。不会覆盖已有的数据。）
        extendFrontmatter: {
            author: {
                name: 'xugaoyi',
                link: 'https://github.com/xugaoyi'
            }
        },

    },

    // 注入到页面<head>中的标签，格式[tagName, { attrName: attrValue }, innerHTML?]
    head: [ // 注入到页面<head> 中的标签，格式[tagName, { attrName: attrValue }, innerHTML?]
        ['link', { rel: 'icon', href: '/img/favicon.ico' }], //favicons，资源放在public文件夹
        ['meta', { name: 'theme-color', content: '#11a8cd' }], // 移动浏览器主题颜色
    ],


    // 插件配置
    plugins: <UserPlugins>[



        // 全文搜索。 ⚠️注意：此插件会在打开网站时多加载部分js文件用于搜索，导致初次访问网站变慢。如在意初次访问速度的话可以不使用此插件！（推荐：vuepress-plugin-thirdparty-search）
        'fulltext-search',


        [
            'one-click-copy', // 代码块复制按钮
            {
                copySelector: ['div[class*="language-"] pre', 'div[class*="aside-code"] aside'], // String or Array
                copyMessage: '复制成功', // default is 'Copy successfully and then paste it for use.'
                duration: 1000, // prompt message display time.
                showInMobile: false, // whether to display on the mobile side, default: false.
            },
        ],

        [
            'vuepress-plugin-zooming', // 放大图片
            {
                selector: '.theme-vdoing-content img:not(.no-zoom)', // 排除class是no-zoom的图片
                options: {
                    bgColor: 'rgba(0,0,0,0.6)',
                },
            },
        ],
        [
            'vuepress-plugin-comment-plus',
            {
                choosen: 'waline',
                // options选项中的所有参数，会传给Waline的配置
                options: {
                    el: '#valine-vuepress-comment',
                    serverURL: 'https://kituin-comments.vercel.app/', //  例如 "https://***.vercel.app/"
                    path: '<%- frontmatter.commentid || frontmatter.permalink %>',
                    emoji: [
                        'https://cdn.jsdelivr.net/gh/walinejs/emojis@1.0.0/weibo',
                        'https://cdn.jsdelivr.net/gh/walinejs/emojis@1.0.0/bilibili',
                    ],
                    placeholder: "请留言"
                }
            }
        ],
        [
            '@vuepress/last-updated', // "上次更新"时间格式
            {
                transformer: (timestamp, lang) => {
                    return dayjs(timestamp).format('YYYY/MM/DD, HH:mm:ss')
                },
            },
        ],
    ],

    markdown: {
        lineNumbers: true,
        extractHeaders: ['h2', 'h3', 'h4', 'h5', 'h6'], // 提取标题到侧边栏的级别，默认['h2', 'h3']
    },

    // 监听文件变化并重新构建
    extraWatchFiles: [
        '.vuepress/config.ts',
        '.vuepress/config/htmlModules.ts',
    ]
})
