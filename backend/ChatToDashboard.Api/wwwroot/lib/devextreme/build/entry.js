// حزمة DevExtreme للرسوم فقط (viz) — تُبنى بـ esbuild لتفادي dx.all.js (4.7MB)
import config from 'devextreme/core/config';
import Chart from 'devextreme/viz/chart';
import PieChart from 'devextreme/viz/pie_chart';
import Funnel from 'devextreme/viz/funnel';
import Bullet from 'devextreme/viz/bullet';
import Sparkline from 'devextreme/viz/sparkline';
import { exportFromMarkup, getMarkup } from 'devextreme/viz/export';
import { registerTheme, currentTheme, refreshTheme } from 'devextreme/viz/themes';
import { registerPalette } from 'devextreme/viz/palette';
import * as localization from 'devextreme/localization';
window.DevExpress = Object.assign(window.DevExpress || {}, {
  config, viz: { dxChart: Chart, dxPieChart: PieChart, dxFunnel: Funnel, dxBullet: Bullet, dxSparkline: Sparkline,
    exportFromMarkup, getMarkup, registerTheme, currentTheme, refreshTheme, registerPalette }, localization });
