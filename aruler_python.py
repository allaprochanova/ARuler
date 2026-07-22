# aruler_python.py — виртуальная линейка (AR) на Python (OpenCV)

import cv2
import numpy as np
import math

class ARuler:
    def __init__(self):
        self.cap = cv2.VideoCapture(0)
        self.points = []
        self.calibration_pixels = 1.0  # пикселей на мм (по умолчанию)
        self.calibrated = False
        self.real_scale = 1.0  # мм на пиксель

    def calibrate(self, pixel_distance, real_mm):
        """Калибровка: pixel_distance пикселей = real_mm мм"""
        if pixel_distance > 0 and real_mm > 0:
            self.real_scale = real_mm / pixel_distance
            self.calibrated = True
            print(f"Калибровка выполнена: 1 пиксель = {self.real_scale:.3f} мм")

    def distance(self, p1, p2):
        return math.hypot(p1[0]-p2[0], p1[1]-p2[1])

    def mouse_callback(self, event, x, y, flags, param):
        if event == cv2.EVENT_LBUTTONDOWN:
            if len(self.points) < 2:
                self.points.append((x, y))
            else:
                self.points = [(x, y)]

    def run(self):
        cv2.namedWindow("ARuler")
        cv2.setMouseCallback("ARuler", self.mouse_callback)
        print("Кликните два раза для измерения. Нажмите 'c' для калибровки, 'r' для сброса, 's' для сохранения, 'q' для выхода.")

        while True:
            ret, frame = self.cap.read()
            if not ret:
                break
            frame = cv2.flip(frame, 1)
            img = frame.copy()

            # Рисуем точки и линию
            if len(self.points) == 1:
                cv2.circle(img, self.points[0], 5, (0, 255, 0), -1)
            elif len(self.points) == 2:
                p1, p2 = self.points
                cv2.circle(img, p1, 5, (0, 255, 0), -1)
                cv2.circle(img, p2, 5, (0, 255, 0), -1)
                cv2.line(img, p1, p2, (0, 0, 255), 2)
                dist_px = self.distance(p1, p2)
                if self.calibrated:
                    dist_mm = dist_px * self.real_scale
                    text = f"{dist_mm:.1f} мм"
                else:
                    text = f"{dist_px:.1f} пикс"
                cv2.putText(img, text, ((p1[0]+p2[0])//2-20, (p1[1]+p2[1])//2-10),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 255), 2)

            cv2.imshow("ARuler", img)
            key = cv2.waitKey(1) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('r'):
                self.points = []
            elif key == ord('c'):
                # Калибровка по двум точкам (предполагаем, что пользователь знает расстояние)
                if len(self.points) == 2:
                    real_mm = float(input("Введите реальное расстояние в мм: "))
                    dist_px = self.distance(self.points[0], self.points[1])
                    self.calibrate(dist_px, real_mm)
                else:
                    print("Сначала поставьте две точки.")
            elif key == ord('s'):
                if len(self.points) == 2 and self.calibrated:
                    with open("measurements.txt", "a") as f:
                        import datetime
                        f.write(f"{datetime.datetime.now()}: {dist_mm:.1f} мм\n")
                    print("Результат сохранён.")
        self.cap.release()
        cv2.destroyAllWindows()

if __name__ == "__main__":
    ar = ARuler()
    ar.run()
