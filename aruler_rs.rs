// aruler_rs.rs — виртуальная линейка (AR) на Rust (консоль)

use std::io::{self, Write, BufRead};
use std::f64;
use std::fs::OpenOptions;
use std::time::SystemTime;

#[derive(Debug, Clone, Copy)]
struct Point { x: f64, y: f64 }

fn distance(p1: Point, p2: Point) -> f64 {
    ((p1.x - p2.x).powi(2) + (p1.y - p2.y).powi(2)).sqrt()
}

fn main() {
    let stdin = io::stdin();
    let mut reader = stdin.lock();
    let mut points: Vec<Point> = Vec::new();
    let mut calibrated = false;
    let mut scale = 0.0; // мм/пикс

    println!("📏 Виртуальная линейка (консольный режим)");
    println!("Команды: add x y, measure, calibrate <real_mm>, reset, save, exit");

    loop {
        print!("> ");
        io::stdout().flush().unwrap();
        let mut line = String::new();
        if reader.read_line(&mut line).is_err() { break; }
        let parts: Vec<&str> = line.trim().split_whitespace().collect();
        if parts.is_empty() { continue; }
        match parts[0] {
            "add" => {
                if parts.len() != 3 {
                    println!("Использование: add x y");
                    continue;
                }
                let x: f64 = parts[1].parse().unwrap_or(0.0);
                let y: f64 = parts[2].parse().unwrap_or(0.0);
                if points.len() < 2 {
                    points.push(Point { x, y });
                    println!("Точка добавлена: ({}, {})", x, y);
                } else {
                    println!("Максимум 2 точки. Используйте reset для очистки.");
                }
            }
            "measure" => {
                if points.len() != 2 {
                    println!("Добавьте две точки (add x y)");
                    continue;
                }
                let dist_px = distance(points[0], points[1]);
                if calibrated {
                    let dist_mm = dist_px * scale;
                    println!("Расстояние: {:.1} мм", dist_mm);
                } else {
                    println!("Расстояние: {:.1} пикс (откалибруйте для мм)", dist_px);
                }
            }
            "calibrate" => {
                if parts.len() != 2 {
                    println!("Использование: calibrate <real_mm>");
                    continue;
                }
                if points.len() != 2 {
                    println!("Добавьте две точки для калибровки.");
                    continue;
                }
                let real_mm: f64 = parts[1].parse().unwrap_or(0.0);
                let dist_px = distance(points[0], points[1]);
                if dist_px == 0.0 {
                    println!("Расстояние между точками равно нулю.");
                    continue;
                }
                scale = real_mm / dist_px;
                calibrated = true;
                println!("Калибровка выполнена: 1 пикс = {:.3} мм", scale);
            }
            "reset" => {
                points.clear();
                println!("Точки сброшены.");
            }
            "save" => {
                if points.len() != 2 || !calibrated {
                    println!("Недостаточно данных для сохранения.");
                    continue;
                }
                let dist_mm = distance(points[0], points[1]) * scale;
                let mut file = OpenOptions::new()
                    .append(true)
                    .create(true)
                    .open("measurements.txt")
                    .unwrap();
                let now = SystemTime::now()
                    .duration_since(SystemTime::UNIX_EPOCH)
                    .unwrap()
                    .as_secs();
                let timestamp = chrono::NaiveDateTime::from_timestamp(now as i64, 0);
                writeln!(file, "{}: {:.1} мм", timestamp.format("%Y-%m-%d %H:%M:%S"), dist_mm).unwrap();
                println!("Результат сохранён.");
            }
            "exit" => {
                println!("До свидания!");
                break;
            }
            _ => println!("Неизвестная команда."),
        }
    }
}
