// aruler_go.go — виртуальная линейка (AR) на Go (консоль)

package main

import (
	"bufio"
	"fmt"
	"math"
	"os"
	"strconv"
	"strings"
	"time"
)

type Point struct{ X, Y float64 }

func distance(p1, p2 Point) float64 {
	return math.Hypot(p1.X-p2.X, p1.Y-p2.Y)
}

func main() {
	scanner := bufio.NewScanner(os.Stdin)
	var points []Point
	var calibrated bool
	var scale float64 // мм/пиксель

	fmt.Println("📏 Виртуальная линейка (консольный режим)")
	fmt.Println("Команды: add x y, measure, calibrate <real_mm>, reset, save, exit")

	for {
		fmt.Print("> ")
		if !scanner.Scan() {
			break
		}
		line := strings.TrimSpace(scanner.Text())
		parts := strings.Fields(line)
		if len(parts) == 0 {
			continue
		}
		switch parts[0] {
		case "add":
			if len(parts) != 3 {
				fmt.Println("Использование: add x y")
				continue
			}
			x, _ := strconv.ParseFloat(parts[1], 64)
			y, _ := strconv.ParseFloat(parts[2], 64)
			if len(points) < 2 {
				points = append(points, Point{x, y})
				fmt.Printf("Точка добавлена: (%v, %v)\n", x, y)
			} else {
				fmt.Println("Максимум 2 точки. Используйте reset для очистки.")
			}
		case "measure":
			if len(points) != 2 {
				fmt.Println("Добавьте две точки (add x y)")
				continue
			}
			distPx := distance(points[0], points[1])
			if calibrated {
				distMm := distPx * scale
				fmt.Printf("Расстояние: %.1f мм\n", distMm)
			} else {
				fmt.Printf("Расстояние: %.1f пикс (откалибруйте для мм)\n", distPx)
			}
		case "calibrate":
			if len(parts) != 2 {
				fmt.Println("Использование: calibrate <real_mm>")
				continue
			}
			if len(points) != 2 {
				fmt.Println("Добавьте две точки для калибровки.")
				continue
			}
			realMm, _ := strconv.ParseFloat(parts[1], 64)
			distPx := distance(points[0], points[1])
			if distPx == 0 {
				fmt.Println("Расстояние между точками равно нулю.")
				continue
			}
			scale = realMm / distPx
			calibrated = true
			fmt.Printf("Калибровка выполнена: 1 пикс = %.3f мм\n", scale)
		case "reset":
			points = nil
			fmt.Println("Точки сброшены.")
		case "save":
			if len(points) != 2 || !calibrated {
				fmt.Println("Недостаточно данных для сохранения.")
				continue
			}
			distMm := distance(points[0], points[1]) * scale
			f, _ := os.OpenFile("measurements.txt", os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
			defer f.Close()
			f.WriteString(fmt.Sprintf("%s: %.1f мм\n", time.Now().Format("2006-01-02 15:04:05"), distMm))
			fmt.Println("Результат сохранён.")
		case "exit":
			fmt.Println("До свидания!")
			return
		default:
			fmt.Println("Неизвестная команда.")
		}
	}
}
