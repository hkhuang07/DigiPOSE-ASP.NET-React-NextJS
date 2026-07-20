Đối với một ứng dụng Single Page Application (SPA) đòi hỏi tốc độ cao, không giật lag và phải giao tiếp liên tục với phần cứng (máy quét mã vạch), Next.js (React) kết hợp với Zustand (quản lý State siêu nhẹ) là lựa chọn tối ưu nhất. Các đoạn log debug trả về terminal nền đen chữ xanh quen thuộc của hệ thống sẽ cho thấy luồng dữ liệu chạy mượt mà thế nào.

Dưới đây là tài liệu Buổi 15 - Tích hợp Front-end hiện đại.

BUỔI 15: TÍCH HỢP FRONT-END HIỆN ĐẠI (NEXT.JS) & QUẢN TRỊ STATE MÁY POS (DIGIPOSE)
Bước 1: Khởi tạo Axios Interceptor (Tự động xoay vòng Refresh Token)
Lỗ hổng lớn nhất của các lập trình viên khi dùng JWT là để người dùng văng ra màn hình đăng nhập khi Access Token hết hạn. Với Axios Interceptor, chúng ta sẽ "đánh chặn" lỗi 401, ngầm gọi API xin Token mới, và tự động gọi lại API cũ mà thu ngân không hề hay biết.

Tạo file utils/axiosClient.ts:

TypeScript
import axios from 'axios';

const axiosClient = axios.create({
    baseURL: 'http://localhost:8080/api/v1',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Request Interceptor: Gắn Access Token vào mỗi Request
axiosClient.interceptors.request.use(
    (config) => {
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken) {
            config.headers['Authorization'] = `Bearer ${accessToken}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: Bắt lỗi 401 và tự động Refresh Token
axiosClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        // Nếu lỗi 401 (Hết hạn Token) và chưa từng thử refresh
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            try {
                const refreshToken = localStorage.getItem('refreshToken');
                const accessToken = localStorage.getItem('accessToken');
                
                // Gọi API cấp lại Token (Đã viết ở Buổi 14)
                const res = await axios.post('http://localhost:8080/api/v1/auth/refresh-token', {
                    accessToken,
                    refreshToken
                });

                const newAccessToken = res.data.accessToken;
                const newRefreshToken = res.data.refreshToken;

                // Cập nhật lại Storage
                localStorage.setItem('accessToken', newAccessToken);
                localStorage.setItem('refreshToken', newRefreshToken);

                // Cập nhật Header và gọi lại API vừa bị lỗi
                originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;
                return axiosClient(originalRequest);
            } catch (refreshError) {
                // Refresh Token cũng hết hạn -> Văng ra màn hình Login
                console.error("[Red Team Alert]: Phiên đăng nhập bị can thiệp hoặc hết hạn tuyệt đối.");
                localStorage.clear();
                window.location.href = '/login';
                return Promise.reject(refreshError);
            }
        }
        return Promise.reject(error);
    }
);

export default axiosClient;
Bước 2: Quản trị State Máy PoS bằng Zustand
Thay vì dùng Redux quá cồng kềnh, ta dùng Zustand để quản lý Giỏ hàng (Đơn nháp) và ID Ca làm việc hiện tại.

Tạo file store/posStore.ts:

TypeScript
import { create } from 'zustand';
import axiosClient from '../utils/axiosClient';

interface CartItem {
    productId: number;
    sku: string;
    productName: string;
    quantity: number;
    basePrice: number;
}

interface PosState {
    shiftId: number | null;
    cart: CartItem[];
    subTotal: number;
    
    // Actions
    setShiftId: (id: number) => void;
    scanBarcode: (sku: string) => Promise<void>;
    updateQuantity: (productId: number, qty: number) => void;
    clearCart: () => void;
}

export const usePosStore = create<PosState>((set, get) => ({
    shiftId: null,
    cart: [],
    subTotal: 0,

    setShiftId: (id: number) => set({ shiftId: id }),

    // Hàm xử lý sự kiện quét mã vạch
    scanBarcode: async (sku: string) => {
        try {
            // Lấy thông tin từ Cache RAM của Server (Đã cấu hình Buổi 14)
            const response = await axiosClient.get(`/products/scan-barcode/${sku}`);
            const product = response.data;
            const currentCart = get().cart;

            // Kiểm tra hàng đã có trong lưới chưa
            const existingItem = currentCart.find(item => item.sku === sku);
            let updatedCart;

            if (existingItem) {
                updatedCart = currentCart.map(item =>
                    item.sku === sku ? { ...item, quantity: item.quantity + 1 } : item
                );
            } else {
                updatedCart = [...currentCart, { ...product, quantity: 1 }];
            }

            // Cập nhật State và tính lại Tổng tiền
            const newTotal = updatedCart.reduce((total, item) => total + (item.basePrice * item.quantity), 0);
            set({ cart: updatedCart, subTotal: newTotal });

        } catch (error) {
            console.error(`Không tìm thấy SKU: ${sku}`);
            alert('Mã vạch không hợp lệ hoặc sản phẩm không tồn tại!');
        }
    },

    updateQuantity: (productId: number, qty: number) => {
        const currentCart = get().cart;
        if (qty <= 0) {
            // Xóa khỏi giỏ nếu SL <= 0
            const filteredCart = currentCart.filter(item => item.productId !== productId);
            const newTotal = filteredCart.reduce((total, item) => total + (item.basePrice * item.quantity), 0);
            set({ cart: filteredCart, subTotal: newTotal });
            return;
        }

        const updatedCart = currentCart.map(item =>
            item.productId === productId ? { ...item, quantity: qty } : item
        );
        const newTotal = updatedCart.reduce((total, item) => total + (item.basePrice * item.quantity), 0);
        set({ cart: updatedCart, subTotal: newTotal });
    },

    clearCart: () => set({ cart: [], subTotal: 0 })
}));
Bước 3: Giao diện Màn hình Thu ngân (Barcode Scanner & Checkout)
Xây dựng một Component React lắng nghe sự kiện nhấn Enter từ máy quét mã vạch vật lý và đẩy luồng thanh toán xuống API nguyên khối (ACID Transaction) đã viết ở Buổi 6.

Tạo file pages/pos/index.tsx:

TypeScript
import React, { useState, useEffect, useRef } from 'react';
import { usePosStore } from '../../store/posStore';
import axiosClient from '../../utils/axiosClient';

export default function PosScreen() {
    const { shiftId, cart, subTotal, scanBarcode, updateQuantity, clearCart, setShiftId } = usePosStore();
    const [barcodeInput, setBarcodeInput] = useState('');
    const inputRef = useRef<HTMLInputElement>(null);

    // Lấy Ca làm việc hiện tại khi load màn hình
    useEffect(() => {
        const fetchCurrentShift = async () => {
            try {
                const res = await axiosClient.get('/shifts/current');
                setShiftId(res.data.shiftId);
            } catch (error) {
                alert("Vui lòng mở ca làm việc trước khi bán hàng.");
                // Chuyển hướng về màn hình Open Shift...
            }
        };
        fetchCurrentShift();
        
        // Luôn focus vào ô quét mã vạch để chờ tín hiệu từ máy quét
        inputRef.current?.focus();
    }, []);

    // Xử lý khi máy quét mã vạch bắn tín hiệu (Kèm phím Enter)
    const handleBarcodeSubmit = async (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' && barcodeInput.trim() !== '') {
            await scanBarcode(barcodeInput.trim());
            setBarcodeInput(''); // Xóa ô input chuẩn bị cho món tiếp theo
            inputRef.current?.focus();
        }
    };

    // Hàm Thanh toán
    const handleCheckout = async () => {
        if (cart.length === 0) {
            alert("Đơn hàng rỗng!"); return;
        }
        if (!shiftId) {
            alert("Lỗi: Không tìm thấy ca làm việc hợp lệ."); return;
        }

        try {
            // 1. Tạo Draft Order trước khi thanh toán
            // Lặp qua từng món trong Zustand Store đẩy lên API AddItem (Đã viết ở Buổi 6)
            for (const item of cart) {
                await axiosClient.post(`/pos/draft-order/${shiftId}/add-item`, {
                    productId: item.productId,
                    quantity: item.quantity
                });
            }

            // 2. Chốt Checkout (Giả sử hệ thống trả về OrderId ở bước 1, ở đây minh họa tĩnh ID = 1)
            // Trong thực tế, API AddItem nên trả về DraftOrderId để dùng cho bước này
            const checkoutRes = await axiosClient.post(`/pos/checkout/1`, {
                customerId: null, // Khách lẻ
                paymentMethod: "Cash"
            });

            alert(checkoutRes.data.message); // Báo thành công
            clearCart(); // Dọn dẹp màn hình chuẩn bị cho khách tiếp theo

        } catch (error: any) {
            alert("Lỗi thanh toán: " + (error.response?.data?.message || "Hệ thống gián đoạn."));
        }
    };

    return (
        <div className="container-fluid bg-dark text-light vh-100 p-4">
            <div className="row h-100">
                
                {/* Khu vực Lưới sản phẩm */}
                <div className="col-md-8 d-flex flex-column">
                    <h3 className="text-success font-monospace mb-4">> DIGIPOSE_TERMINAL_READY</h3>
                    
                    <input 
                        type="text" 
                        ref={inputRef}
                        className="form-control bg-black text-success border-success font-monospace mb-3" 
                        placeholder="Quét mã vạch (SKU) vào đây..." 
                        value={barcodeInput}
                        onChange={(e) => setBarcodeInput(e.target.value)}
                        onKeyDown={handleBarcodeSubmit}
                        autoFocus
                    />

                    <div className="table-responsive flex-grow-1 bg-black p-3 border border-secondary rounded">
                        <table className="table table-dark table-borderless text-light font-monospace">
                            <thead>
                                <tr className="border-bottom border-success">
                                    <th>SKU</th>
                                    <th>SẢN PHẨM</th>
                                    <th className="text-center">SL</th>
                                    <th className="text-end">ĐƠN GIÁ</th>
                                    <th className="text-end">THÀNH TIỀN</th>
                                </tr>
                            </thead>
                            <tbody>
                                {cart.map((item) => (
                                    <tr key={item.productId}>
                                        <td className="text-secondary">{item.sku}</td>
                                        <td>{item.productName}</td>
                                        <td className="text-center">
                                            <input 
                                                type="number" 
                                                className="form-control form-control-sm bg-dark text-light border-0 text-center w-50 mx-auto"
                                                value={item.quantity}
                                                onChange={(e) => updateQuantity(item.productId, parseInt(e.target.value))}
                                            />
                                        </td>
                                        <td className="text-end">{item.basePrice.toLocaleString('vi-VN')}</td>
                                        <td className="text-end text-warning">{(item.basePrice * item.quantity).toLocaleString('vi-VN')}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Khu vực Thanh toán */}
                <div className="col-md-4 border-start border-secondary d-flex flex-column p-4">
                    <h4 className="text-muted mb-4">THÔNG TIN THANH TOÁN</h4>
                    
                    <div className="flex-grow-1">
                        <div className="d-flex justify-content-between mb-3 fs-5">
                            <span>TỔNG TIỀN:</span>
                            <span className="text-danger fw-bold">{subTotal.toLocaleString('vi-VN')} VNĐ</span>
                        </div>
                        <div className="d-flex justify-content-between mb-3">
                            <span className="text-muted">Ca làm việc ID:</span>
                            <span className="font-monospace text-info">#{shiftId || 'NULL'}</span>
                        </div>
                    </div>

                    <button 
                        className="btn btn-success btn-lg w-100 font-monospace rounded-0 py-3"
                        onClick={handleCheckout}
                    >
                        [ ENTER ] THANH TOÁN
                    </button>
                    <button 
                        className="btn btn-outline-danger w-100 mt-3 font-monospace rounded-0"
                        onClick={clearCart}
                    >
                        [ ESC ] HỦY GIAO DỊCH
                    </button>
                </div>

            </div>
        </div>
    );
}