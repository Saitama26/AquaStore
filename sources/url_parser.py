#!/usr/bin/env python3
"""
URL Parser - парсер ссылок с поддержкой командной строки
Использование: python url_parser.py <url1> <url2> ... --output <папка>
"""

import argparse
import os
import sys
import hashlib
from pathlib import Path
from urllib.parse import urlparse, parse_qs
import requests
from bs4 import BeautifulSoup


def parse_url(url, output_dir):
    """Парсит URL и сохраняет содержимое"""
    try:
        print(f"Парсинг: {url}")
        
        # Отправляем запрос
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        }
        response = requests.get(url, headers=headers, timeout=30)
        response.raise_for_status()
        
        # Парсим HTML
        soup = BeautifulSoup(response.content, 'html.parser')
        
        # Генерируем имя файла из URL
        parsed = urlparse(url)
        filename = parsed.netloc.replace('.', '_') + parsed.path.replace('/', '_')
        
        # Добавляем query параметры в имя файла
        if parsed.query:
            # Создаем короткий хеш от query параметров для уникальности
            query_hash = hashlib.md5(parsed.query.encode()).hexdigest()[:8]
            filename += f'_query_{query_hash}'
        
        if not filename.endswith('.html'):
            filename += '.html'
        
        # Сохраняем
        output_path = Path(output_dir) / filename
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(soup.prettify())
        
        print(f"✓ Сохранено: {output_path}")
        return True
        
    except requests.RequestException as e:
        print(f"✗ Ошибка при загрузке {url}: {e}")
        return False
    except Exception as e:
        print(f"✗ Ошибка при обработке {url}: {e}")
        return False


def main():
    parser = argparse.ArgumentParser(
        description='Парсер ссылок - загружает и сохраняет содержимое веб-страниц',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Примеры использования:
  python url_parser.py https://example.com --output ./parsed
  python url_parser.py https://site1.com https://site2.com -o ./data
        """
    )
    
    parser.add_argument(
        'urls',
        nargs='+',
        help='URL-адреса для парсинга'
    )
    
    parser.add_argument(
        '-o', '--output',
        default='./parsed_urls',
        help='Папка для сохранения результатов (по умолчанию: ./parsed_urls)'
    )
    
    args = parser.parse_args()
    
    # Создаем выходную директорию
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)
    print(f"Выходная папка: {output_dir.absolute()}\n")
    
    # Парсим все URL
    success_count = 0
    for url in args.urls:
        if parse_url(url, output_dir):
            success_count += 1
        print()
    
    # Итоги
    print(f"Завершено: {success_count}/{len(args.urls)} успешно")


if __name__ == '__main__':
    main()
