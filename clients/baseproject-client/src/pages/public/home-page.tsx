import { Link } from 'react-router-dom';
import { Button } from '../../components/ui/button';
import { useQuery } from '@tanstack/react-query';
import { getAllCategories } from '../../features/categories/api';
import { Loader2 } from 'lucide-react';

export function HomePage() {
  const { data: categories = [], isLoading: isCategoriesLoading } = useQuery({
    queryKey: ['categories', 'all'],
    queryFn: getAllCategories
  });

  return (
    <div className="bg-background">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="space-y-12">
          <header className="space-y-3">
            <h1 className="text-3xl font-semibold text-foreground sm:text-4xl">BaseProject</h1>
            <p className="max-w-2xl text-sm text-muted-foreground sm:text-base">
              Modern, ölçeklenebilir ve güvenli bir proje temeli.
            </p>
          </header>

          <section className="space-y-3">
            <h2 className="text-xs font-semibold uppercase tracking-[0.25em] text-muted-foreground/80">Kategoriler</h2>
            <div className="flex flex-wrap gap-2">
              {isCategoriesLoading ? (
                Array.from({ length: 6 }).map((_, index) => (
                  <div key={index} className="h-9 w-20 animate-pulse rounded-full bg-muted/60" />
                ))
              ) : Array.isArray(categories) && categories.length > 0 ? (
                categories.map((category) => (
                  <Button
                    key={category.id}
                    type="button"
                    size="sm"
                    variant="outline"
                    className="rounded-full border px-4 py-2 text-sm font-medium"
                  >
                    {category.name}
                  </Button>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">Henüz kategori bulunmuyor.</p>
              )}
            </div>
          </section>

          <section className="space-y-6">
            <div className="rounded-2xl border border-border/60 bg-card p-10 text-center">
              <h2 className="text-2xl font-semibold text-foreground mb-4">Hoş Geldiniz</h2>
              <p className="text-sm text-muted-foreground mb-6">
                Bu, BaseProject'in ana sayfasıdır. Projenizi bu temel üzerine inşa edebilirsiniz.
              </p>
              <div className="flex gap-3 justify-center">
                <Button asChild variant="default">
                  <Link to="/login">Giriş Yap</Link>
                </Button>
                <Button asChild variant="outline">
                  <Link to="/register">Kayıt Ol</Link>
                </Button>
              </div>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
