// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function getAnimationSource(triggerElement) {
    if (!triggerElement) {
        return document.getElementById('main-product-image');
    }

    const detailsRoot = triggerElement.closest('.pd-card') || triggerElement.closest('.product-details');
    if (detailsRoot) {
        const detailsImage = detailsRoot.querySelector('#main-product-image');
        if (detailsImage) {
            return detailsImage;
        }
    }

    const cardImage = triggerElement.closest('.product-card')?.querySelector('.product-image');
    return cardImage || document.getElementById('main-product-image');
}

function animateProductToCart(triggerElement) {
    const sourceImage = getAnimationSource(triggerElement);
    const cartButton = document.querySelector('.cart-button');

    if (!cartButton) {
        return Promise.resolve();
    }

    const sourceRect = sourceImage instanceof HTMLImageElement
        ? sourceImage.getBoundingClientRect()
        : triggerElement?.getBoundingClientRect();
    if (!sourceRect) {
        return Promise.resolve();
    }

    const cartRect = cartButton.getBoundingClientRect();
    const flyImage = document.createElement('img');
    flyImage.src = sourceImage instanceof HTMLImageElement
        ? (sourceImage.currentSrc || sourceImage.src)
        : '/images/water-filter.svg';
    flyImage.alt = '';
    flyImage.className = 'cart-fly-image';
    const startSize = Math.max(36, Math.min(72, sourceRect.width));
    const startX = sourceRect.left + (sourceRect.width / 2) - (startSize / 2);
    const startY = sourceRect.top + (sourceRect.height / 2) - (startSize / 2);
    const endX = cartRect.left + (cartRect.width / 2) - 8;
    const endY = cartRect.top + (cartRect.height / 2) - 8;
    const middleX = (startX + endX) / 2;
    const middleY = Math.min(startY, endY) - 120;

    flyImage.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: ${startSize}px;
        height: ${startSize}px;
        border-radius: 10px;
        object-fit: cover;
        pointer-events: none;
        z-index: 10001;
        transform: translate3d(${startX}px, ${startY}px, 0) scale(1);
        opacity: 0.95;
        box-shadow: 0 10px 24px rgba(0,0,0,0.22);
    `;

    document.body.appendChild(flyImage);

    return new Promise((resolve) => {
        let completed = false;
        const animation = flyImage.animate([
            { transform: `translate3d(${startX}px, ${startY}px, 0) scale(1)`, opacity: 0.95 },
            { transform: `translate3d(${middleX}px, ${middleY}px, 0) scale(0.72)`, opacity: 0.85, offset: 0.55 },
            { transform: `translate3d(${endX}px, ${endY}px, 0) scale(0.15)`, opacity: 0.08 }
        ], {
            duration: 650,
            easing: 'cubic-bezier(0.22, 0.7, 0.26, 1)',
            fill: 'forwards'
        });

        const finish = () => {
            if (completed) {
                return;
            }
            completed = true;
            flyImage.remove();
            cartButton.classList.add('cart-bump');
            setTimeout(() => cartButton.classList.remove('cart-bump'), 250);
            resolve();
        };

        animation.addEventListener('finish', finish, { once: true });
        setTimeout(finish, 750);
    });
}

// Функция для добавления товара в корзину
async function addToCart(productId, triggerElement = null, options = {}) {
    const shouldShowSuccess = options.showSuccessNotification !== false;

    try {
        const response = await fetch(`/Cart/Add?productId=${productId}&quantity=1`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            await animateProductToCart(triggerElement);
            if (shouldShowSuccess) {
            showNotification('Товар добавлен в корзину', 'success');
            }
            // Обновить счетчик корзины, если есть
            updateCartCount();
            return true;
        } else if (response.status === 401 || response.status === 403) {
            showNotification('Войдите в аккаунт, чтобы добавить товар', 'info');
            return false;
        } else {
            let errorMessage = 'Ошибка при добавлении товара в корзину';
            try {
                if (response.headers.get('content-type')?.includes('application/json')) {
                    const data = await response.json();
                    if (data?.message) {
                        errorMessage = data.message;
                    }
                }
            } catch {
                // ignore parse errors
            }

            showNotification(errorMessage, 'error');
            return false;
        }
    } catch (error) {
        console.error('Ошибка при добавлении в корзину:', error);
        showNotification('Произошла ошибка. Попробуйте позже.', 'error');
        return false;
    }
}

async function buyNow(productId, triggerElement = null) {
    const isAdded = await addToCart(productId, triggerElement, { showSuccessNotification: false });
    if (isAdded) {
        const params = new URLSearchParams();
        params.append('productIds', productId);
        params.append('buyNow', 'true');
        window.location.href = `/Cart/Checkout?${params.toString()}`;
    }
}

// Функция для обновления счетчика корзины
async function updateCartCount() {
    try {
        const response = await fetch('/Cart/GetCount');
        if (response.ok) {
            const count = parseInt(await response.text(), 10) || 0;
            const cartBadge = document.querySelector('.cart-button-count');
            if (cartBadge) {
                cartBadge.textContent = `${count}`;
                cartBadge.classList.toggle('is-hidden', count <= 0);
            }
        }
    } catch (error) {
        console.error('Ошибка при обновлении счетчика корзины:', error);
    }
}

// Функция для показа уведомлений
function showNotification(message, type = 'info') {
    // Простая реализация уведомлений (можно заменить на toast библиотеку)
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 15px 20px;
        background: ${type === 'success' ? '#2f8cff' : type === 'error' ? '#ff6b6b' : '#3fb4ff'};
        color: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 10000;
        animation: slideIn 0.3s ease;
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Добавить стили для анимации уведомлений
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateY(100%);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }
    @keyframes slideOut {
        from {
            transform: translateY(0);
            opacity: 1;
        }
        to {
            transform: translateY(100%);
            opacity: 0;
        }
    }
    @keyframes cartBump {
        0% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.12);
        }
        100% {
            transform: scale(1);
        }
    }
    .cart-button.cart-bump {
        animation: cartBump 0.22s ease;
    }
`;
document.head.appendChild(style);

function initNavigationDrawer() {
    const burgerButton = document.querySelector('.burger-button');
    const drawer = document.getElementById('navDrawer');
    const backdrop = document.getElementById('navBackdrop');
    const closeButton = drawer?.querySelector('.nav-drawer-close');

    if (!burgerButton || !drawer || !backdrop) {
        return;
    }

    const openDrawer = () => {
        drawer.classList.add('open');
        backdrop.classList.add('show');
        document.body.classList.add('nav-locked');
        burgerButton.setAttribute('aria-expanded', 'true');
        drawer.setAttribute('aria-hidden', 'false');
    };

    const closeDrawer = () => {
        drawer.classList.remove('open');
        backdrop.classList.remove('show');
        document.body.classList.remove('nav-locked');
        burgerButton.setAttribute('aria-expanded', 'false');
        drawer.setAttribute('aria-hidden', 'true');
    };

    burgerButton.addEventListener('click', (event) => {
        event.preventDefault();
        if (drawer.classList.contains('open')) {
            closeDrawer();
        } else {
            openDrawer();
        }
    });

    backdrop.addEventListener('click', closeDrawer);
    closeButton?.addEventListener('click', closeDrawer);

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            closeDrawer();
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initNavigationDrawer();
    initInfiniteScroll();
    updateCartCount();
});

let isLoadingProducts = false;

function initInfiniteScroll() {
    const container = document.getElementById('productsContainer');
    if (!container) {
        return;
    }

    const pagination = document.getElementById('pagination');
    if (pagination) {
        pagination.style.display = 'none';
    }

    const loadMore = async () => {
        if (isLoadingProducts) return;

        const currentPage = parseInt(container.dataset.page || '1', 10);
        const totalPages = parseInt(container.dataset.totalPages || '1', 10);

        if (currentPage >= totalPages) {
            return;
        }

        isLoadingProducts = true;
        const nextPage = currentPage + 1;

        const params = new URLSearchParams(window.location.search);
        params.set('pageNumber', nextPage.toString());

        const response = await fetch(`/home/load-more?${params.toString()}`, {
            method: 'GET'
        });

        if (response.ok) {
            const html = await response.text();
            const grid = container.querySelector('.products-grid');
            if (grid) {
                grid.insertAdjacentHTML('beforeend', html);
            } else {
                container.insertAdjacentHTML('beforeend', html);
            }
            container.dataset.page = nextPage.toString();
        }

        isLoadingProducts = false;
    };

    const onScroll = () => {
        const threshold = 300;
        if ((window.innerHeight + window.scrollY) >= (document.body.offsetHeight - threshold)) {
            loadMore();
        }
    };

    window.addEventListener('scroll', onScroll);
}
