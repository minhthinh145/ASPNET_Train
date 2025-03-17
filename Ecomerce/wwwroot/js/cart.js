$(document).ready(function () {
    console.log("Cart.js loaded!");

    $('.btn-plus, .btn-minus').off('click').on('click', function () {
        console.log("Button clicked!");

        var $input = $(this).closest('.quantity').find('.quantity-input');
        var oldValue = parseInt($input.val());
        var productId = $(this).closest('.quantity').data('id'); // Lấy ID sản phẩm từ data-id
        var price = parseFloat($(this).closest('.quantity').data('price')); // Lấy đơn giá từ data-price
        var newValue = oldValue;

        // Xử lý tăng giảm
        if ($(this).hasClass('btn-plus')) {
            newValue = oldValue + 1;
        } else if ($(this).hasClass('btn-minus')) {
            newValue = oldValue > 1 ? oldValue - 1 : 1;
        }

        console.log("Product ID: " + productId + ", New Quantity: " + newValue);

        // Cập nhật số lượng hiển thị
        $input.val(newValue);

        // Tính lại thành tiền cho sản phẩm
        var newItemTotal = newValue * price;
        $('#total-' + productId).text('$ ' + newItemTotal.toFixed(2));

        // Gửi Ajax cập nhật server
        $.ajax({
            url: '/Cart/UpdateCart', // ✅ Đúng Action
            type: 'POST',
            data: {
                productId: productId,
                quantity: newValue
            },
            success: function (response) {
                console.log(response);
                if (response.success) {
                    // Cập nhật tổng tiền và tổng số lượng
                    $('#total-amount').text('$ ' + response.total.toFixed(2));
                    $('#cart-quantity').text(response.totalQuantity);
                    $('#total-' + productId).text('$ ' + response.itemTotal.toFixed(2)); // cập nhật thành tiền
                } else {
                    alert('Cập nhật thất bại!');
                }
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);
                alert('Đã xảy ra lỗi khi cập nhật giỏ hàng!');
                }
        });
    });
});
