import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { FolderKanban, Users, Shield, Activity as ActivityIcon, Sparkles } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { StatCard } from '../../components/dashboard/stat-card';
import { ActivityFeed, Activity } from '../../components/dashboard/activity-feed';
import { AiInsightsCard } from '../../components/dashboard/ai-insights-card';
import { fetchStatistics, fetchRecentActivities, fetchAiInsights } from '../../features/dashboard/api';
import { usePermission } from '../../hooks/use-permission';
import { Permissions } from '../../lib/permissions';
import { useState } from 'react';

export function DashboardPage() {
  const { hasPermission } = usePermission();
  const canViewAIInsights = hasPermission(Permissions.DashboardAIInsights);
  const queryClient = useQueryClient();
  const [isLoadingInsights, setIsLoadingInsights] = useState(false);

  // ✅ Dashboard istatistikleri - sık değişen veri için staleTime override
  // Global staleTime (5 dakika) yerine 30 saniye kullanıyoruz
  const { data: stats, isLoading } = useQuery({
    queryKey: ['dashboard-statistics'],
    queryFn: fetchStatistics,
    staleTime: 30 * 1000, // 30 saniye (global 5 dakika yerine override)
    refetchInterval: 30000 // Her 30 saniyede bir güncelle
  });

  // ✅ Son aktiviteler - sık değişen veri için staleTime override
  const { data: recentActivities = [], isLoading: isLoadingActivities } = useQuery({
    queryKey: ['recent-activities'],
    queryFn: () => fetchRecentActivities(10),
    staleTime: 30 * 1000, // 30 saniye (global 5 dakika yerine override)
    refetchInterval: 30000 // Her 30 saniyede bir güncelle
  });

  // AI Insights - sadece enabled olduğunda query çalışacak, manuel tetikleme için
  const { data: aiInsights, isLoading: isLoadingInsightsQuery } = useQuery({
    queryKey: ['dashboard-ai-insights'],
    queryFn: fetchAiInsights,
    enabled: false, // Otomatik çalışmasın, manuel buton ile tetiklenecek
  });

  const handleLoadAIInsights = async () => {
    setIsLoadingInsights(true);
    try {
      await queryClient.fetchQuery({
        queryKey: ['dashboard-ai-insights'],
        queryFn: fetchAiInsights,
      });
    } catch (error) {
      console.error('AI insights yüklenirken hata:', error);
    } finally {
      setIsLoadingInsights(false);
    }
  };

  const isLoadingAIInsights = isLoadingInsights || isLoadingInsightsQuery;

  // API verisini bileşen formatına dönüştür
  const activities: Activity[] = recentActivities.map(activity => ({
    id: activity.id,
    activityType: activity.activityType,
    title: activity.title,
    timestamp: activity.timestamp,
    userName: activity.userName
  }));

  if (isLoading || isLoadingActivities) {
    return (
      <div className="space-y-8">
        <Card className="p-6">
          <div className="flex items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
            <span className="ml-3 text-muted-foreground">Yükleniyor...</span>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <motion.div
        className="flex flex-col gap-4 rounded-xl border bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 p-6 shadow-sm lg:flex-row lg:items-center lg:justify-between"
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.25 }}
      >
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Yönetim Paneline Hoş Geldiniz</h1>
          <p className="mt-2 max-w-xl text-sm text-muted-foreground">
            BaseProject içeriklerinizi kolayca yönetin, istatistiklerinizi takip edin.
          </p>
        </div>
      </motion.div>

      {/* İstatistik Kartları */}
      <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-4">
        <StatCard
          title="Toplam Kategoriler"
          value={stats?.totalCategories ?? 0}
          description="Aktif kategori sayısı"
          icon={FolderKanban}
          color="purple"
          delay={0}
        />
        <StatCard
          title="Toplam Kullanıcılar"
          value={stats?.totalUsers ?? 0}
          description="Kayıtlı kullanıcı sayısı"
          icon={Users}
          color="blue"
          delay={0.1}
        />
        <StatCard
          title="Toplam Roller"
          value={stats?.totalRoles ?? 0}
          description="Sistem rolleri"
          icon={Shield}
          color="green"
          delay={0.2}
        />
        <StatCard
          title="Aktiviteler"
          value={recentActivities.length}
          description="Son aktivite sayısı"
          icon={ActivityIcon}
          color="yellow"
          delay={0.3}
        />
      </div>

      {/* AI İçgörüleri Butonu */}
      {canViewAIInsights && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-primary" />
              AI İçgörüleri
            </CardTitle>
            <CardDescription>
              Yapay zeka destekli sistem analizi ve öneriler
            </CardDescription>
          </CardHeader>
          <CardContent>
            {!aiInsights && !isLoadingAIInsights && (
              <div className="flex items-center justify-between p-4 rounded-lg border bg-muted/50">
                <div>
                  <p className="text-sm font-medium">AI içgörüleri henüz yüklenmedi</p>
                  <p className="text-xs text-muted-foreground mt-1">
                    Sistem verilerinizi analiz edip trendler, uyarılar ve öneriler üretmek için butona tıklayın.
                  </p>
                </div>
                <Button onClick={handleLoadAIInsights} className="ml-4">
                  <Sparkles className="mr-2 h-4 w-4" />
                  İçgörüleri Yükle
                </Button>
              </div>
            )}
            {(aiInsights || isLoadingAIInsights) && (
              <div className="space-y-4">
                <div className="flex justify-end">
                  <Button 
                    onClick={handleLoadAIInsights} 
                    disabled={isLoadingAIInsights}
                    variant="outline"
                    size="sm"
                  >
                    {isLoadingAIInsights ? (
                      <>
                        <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                        Yükleniyor...
                      </>
                    ) : (
                      <>
                        <Sparkles className="mr-2 h-4 w-4" />
                        Yenile
                      </>
                    )}
                  </Button>
                </div>
                <AiInsightsCard
                  trends={aiInsights?.trends ?? []}
                  alerts={aiInsights?.alerts ?? []}
                  recommendations={aiInsights?.recommendations ?? []}
                  isLoading={isLoadingAIInsights}
                  delay={0.4}
                />
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Hızlı Aksiyonlar ve Aktiviteler */}
      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FolderKanban className="h-5 w-5 text-primary" />
              Hızlı Aksiyonlar
            </CardTitle>
            <CardDescription>Sık kullanılan işlemler</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link to="/admin/categories">
                <FolderKanban className="mr-2 h-4 w-4" />
                Kategorileri Yönet
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link to="/admin/users">
                <Users className="mr-2 h-4 w-4" />
                Kullanıcıları Yönet
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link to="/admin/roles">
                <Shield className="mr-2 h-4 w-4" />
                Rolleri Yönet
              </Link>
            </Button>
            <Button variant="outline" className="w-full justify-start" asChild>
              <Link to="/admin/activity-logs">
                <ActivityIcon className="mr-2 h-4 w-4" />
                Aktivite Logları
              </Link>
            </Button>
          </CardContent>
        </Card>

        <div className="lg:col-span-2">
          <ActivityFeed activities={activities} delay={0.6} />
        </div>
      </div>
    </div>
  );
}
