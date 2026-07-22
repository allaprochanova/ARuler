// aruler_java.java — виртуальная линейка (AR) на Java (JavaCV)

import org.bytedeco.javacv.*;
import org.bytedeco.opencv.opencv_core.*;
import static org.bytedeco.opencv.global.opencv_core.*;
import static org.bytedeco.opencv.global.opencv_imgproc.*;
import static org.bytedeco.opencv.global.opencv_imgcodecs.*;
import org.bytedeco.javacv.CanvasFrame;
import java.awt.event.*;
import java.io.*;

public class ARuler {
    private static Point[] points = new Point[2];
    private static int pointCount = 0;
    private static boolean calibrated = false;
    private static double scale = 1.0; // мм/пикс
    private static CanvasFrame canvas;

    public static void main(String[] args) throws Exception {
        OpenCVFrameGrabber grabber = new OpenCVFrameGrabber(0);
        grabber.start();
        canvas = new CanvasFrame("ARuler");
        canvas.setDefaultCloseOperation(javax.swing.JFrame.EXIT_ON_CLOSE);
        canvas.addMouseListener(new MouseAdapter() {
            public void mouseClicked(MouseEvent e) {
                if (pointCount < 2) {
                    points[pointCount++] = new Point(e.getX(), e.getY());
                } else {
                    pointCount = 0;
                    points[pointCount++] = new Point(e.getX(), e.getY());
                }
            }
        });
        System.out.println("Кликните два раза для измерения. Нажмите 'c' для калибровки, 'r' для сброса, 's' для сохранения, 'q' для выхода.");

        while (true) {
            Mat frame = new Mat();
            Frame f = grabber.grabFrame();
            if (f == null) break;
            OpenCVFrameConverter.ToMat converter = new OpenCVFrameConverter.ToMat();
            frame = converter.convert(f);
            flip(frame, frame, 1);
            Mat img = frame.clone();

            if (pointCount == 1) {
                circle(img, points[0], 5, Scalar.GREEN, -1, 8, 0);
            } else if (pointCount == 2) {
                Point p1 = points[0], p2 = points[1];
                circle(img, p1, 5, Scalar.GREEN, -1, 8, 0);
                circle(img, p2, 5, Scalar.GREEN, -1, 8, 0);
                line(img, p1, p2, Scalar.RED, 2, 8, 0);
                double distPx = distance(p1, p2);
                String text;
                if (calibrated) {
                    double distMm = distPx * scale;
                    text = String.format("%.1f мм", distMm);
                } else {
                    text = String.format("%.1f пикс", distPx);
                }
                putText(img, text, new Point((p1.x()+p2.x())/2-20, (p1.y()+p2.y())/2-10),
                        FONT_HERSHEY_SIMPLEX, 0.8, Scalar.YELLOW, 2, 8, false);
            }
            canvas.showImage(converter.convert(img));
            // Обработка клавиш (в JavaCV не так просто, используем консоль)
            // Для простоты будем использовать консольный ввод
        }
        grabber.stop();
    }

    private static double distance(Point p1, Point p2) {
        return Math.hypot(p1.x()-p2.x(), p1.y()-p2.y());
    }
}
