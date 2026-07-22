// aruler_js.js — виртуальная линейка (AR) на JavaScript (Node.js)

const readline = require('readline');
const fs = require('fs');
const { DateTime } = require('luxon');

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    prompt: '> '
});

let points = [];
let calibrated = false;
let scale = 0.0; // мм/пикс

function distance(p1, p2) {
    return Math.hypot(p1.x - p2.x, p1.y - p2.y);
}

console.log('📏 Виртуальная линейка (консольный режим)');
console.log('Команды: add x y, measure, calibrate <real_mm>, reset, save, exit');
rl.prompt();

rl.on('line', (line) => {
    const parts = line.trim().split(/\s+/);
    if (parts.length === 0) { rl.prompt(); return; }
    const cmd = parts[0];
    switch (cmd) {
        case 'add': {
            if (parts.length !== 3) {
                console.log('Использование: add x y');
                break;
            }
            const x = parseFloat(parts[1]);
            const y = parseFloat(parts[2]);
            if (isNaN(x) || isNaN(y)) {
                console.log('Некорректные координаты.');
                break;
            }
            if (points.length < 2) {
                points.push({x, y});
                console.log(`Точка добавлена: (${x}, ${y})`);
            } else {
                console.log('Максимум 2 точки. Используйте reset для очистки.');
            }
            break;
        }
        case 'measure': {
            if (points.length !== 2) {
                console.log('Добавьте две точки (add x y)');
                break;
            }
            const distPx = distance(points[0], points[1]);
            if (calibrated) {
                const distMm = distPx * scale;
                console.log(`Расстояние: ${distMm.toFixed(1)} мм`);
            } else {
                console.log(`Расстояние: ${distPx.toFixed(1)} пикс (откалибруйте для мм)`);
            }
            break;
        }
        case 'calibrate': {
            if (parts.length !== 2) {
                console.log('Использование: calibrate <real_mm>');
                break;
            }
            if (points.length !== 2) {
                console.log('Добавьте две точки для калибровки.');
                break;
            }
            const realMm = parseFloat(parts[1]);
            if (isNaN(realMm) || realMm <= 0) {
                console.log('Введите положительное число.');
                break;
            }
            const distPx = distance(points[0], points[1]);
            if (distPx === 0) {
                console.log('Расстояние между точками равно нулю.');
                break;
            }
            scale = realMm / distPx;
            calibrated = true;
            console.log(`Калибровка выполнена: 1 пикс = ${scale.toFixed(3)} мм`);
            break;
        }
        case 'reset': {
            points = [];
            console.log('Точки сброшены.');
            break;
        }
        case 'save': {
            if (points.length !== 2 || !calibrated) {
                console.log('Недостаточно данных для сохранения.');
                break;
            }
            const distMm = distance(points[0], points[1]) * scale;
            const now = DateTime.now().toISO();
            fs.appendFileSync('measurements.txt', `${now}: ${distMm.toFixed(1)} мм\n`);
            console.log('Результат сохранён.');
            break;
        }
        case 'exit': {
            console.log('До свидания!');
            rl.close();
            return;
        }
        default:
            console.log('Неизвестная команда.');
    }
    rl.prompt();
}).on('close', () => process.exit(0));
