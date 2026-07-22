// aruler_cpp.cpp — виртуальная линейка (AR) на C++ (OpenCV)

#include <opencv2/opencv.hpp>
#include <iostream>
#include <cmath>
#include <vector>

using namespace cv;
using namespace std;

vector<Point> points;
bool calibrated = false;
double scale = 1.0; // мм на пиксель

void mouseCallback(int event, int x, int y, int flags, void* userdata) {
    if (event == EVENT_LBUTTONDOWN) {
        if (points.size() < 2) {
            points.push_back(Point(x, y));
        } else {
            points.clear();
            points.push_back(Point(x, y));
        }
    }
}

double distance(const Point& p1, const Point& p2) {
    return sqrt(pow(p1.x - p2.x, 2) + pow(p1.y - p2.y, 2));
}

int main() {
    VideoCapture cap(0);
    if (!cap.isOpened()) {
        cerr << "Не удалось открыть камеру" << endl;
        return -1;
    }
    namedWindow("ARuler");
    setMouseCallback("ARuler", mouseCallback);
    cout << "Кликните два раза для измерения. Нажмите 'c' для калибровки, 'r' для сброса, 's' для сохранения, 'q' для выхода." << endl;

    while (true) {
        Mat frame, img;
        cap >> frame;
        if (frame.empty()) break;
        flip(frame, frame, 1);
        frame.copyTo(img);

        if (points.size() == 1) {
            circle(img, points[0], 5, Scalar(0, 255, 0), -1);
        } else if (points.size() == 2) {
            Point p1 = points[0], p2 = points[1];
            circle(img, p1, 5, Scalar(0, 255, 0), -1);
            circle(img, p2, 5, Scalar(0, 255, 0), -1);
            line(img, p1, p2, Scalar(0, 0, 255), 2);
            double dist_px = distance(p1, p2);
            string text;
            if (calibrated) {
                double dist_mm = dist_px * scale;
                text = to_string(dist_mm) + " мм";
            } else {
                text = to_string(dist_px) + " пикс";
            }
            putText(img, text, Point((p1.x+p2.x)/2-20, (p1.y+p2.y)/2-10),
                    FONT_HERSHEY_SIMPLEX, 0.8, Scalar(0, 255, 255), 2);
        }
        imshow("ARuler", img);
        char key = waitKey(1) & 0xFF;
        if (key == 'q') break;
        else if (key == 'r') points.clear();
        else if (key == 'c') {
            if (points.size() == 2) {
                double real_mm;
                cout << "Введите реальное расстояние в мм: ";
                cin >> real_mm;
                double dist_px = distance(points[0], points[1]);
                scale = real_mm / dist_px;
                calibrated = true;
                cout << "Калибровка выполнена: 1 пиксель = " << scale << " мм" << endl;
            } else {
                cout << "Сначала поставьте две точки." << endl;
            }
        } else if (key == 's') {
            if (points.size() == 2 && calibrated) {
                ofstream file("measurements.txt", ios::app);
                time_t now = time(0);
                file << ctime(&now) << ": " << distance(points[0], points[1]) * scale << " мм" << endl;
                file.close();
                cout << "Результат сохранён." << endl;
            }
        }
    }
    cap.release();
    destroyAllWindows();
    return 0;
}
