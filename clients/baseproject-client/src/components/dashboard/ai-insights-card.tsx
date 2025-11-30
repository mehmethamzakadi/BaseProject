import { motion } from 'framer-motion';
import { Sparkles, TrendingUp, TrendingDown, AlertTriangle, AlertCircle, Lightbulb, ArrowRight } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Badge } from '../ui/badge';
import { Link } from 'react-router-dom';
import { Button } from '../ui/button';
import { InsightTrend, InsightAlert, InsightRecommendation } from '../../features/dashboard/types';

interface AiInsightsCardProps {
  trends: InsightTrend[];
  alerts: InsightAlert[];
  recommendations: InsightRecommendation[];
  isLoading?: boolean;
  delay?: number;
}

export function AiInsightsCard({ trends, alerts, recommendations, isLoading, delay = 0 }: AiInsightsCardProps) {

  const getSeverityIcon = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical':
      case 'high':
        return <AlertTriangle className="h-4 w-4" />;
      case 'medium':
        return <AlertCircle className="h-4 w-4" />;
      default:
        return <AlertCircle className="h-4 w-4" />;
    }
  };

  const getCategoryColor = (category: string) => {
    switch (category.toLowerCase()) {
      case 'performance':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'security':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'content':
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200';
      case 'user_experience':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'maintenance':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary animate-pulse" />
            AI İçgörüleri
          </CardTitle>
          <CardDescription>Yapay zeka destekli analiz yükleniyor...</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center py-8">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
          </div>
        </CardContent>
      </Card>
    );
  }

  const hasData = trends.length > 0 || alerts.length > 0 || recommendations.length > 0;

  if (!hasData) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary" />
            AI İçgörüleri
          </CardTitle>
          <CardDescription>Henüz içgörü bulunmuyor</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Daha fazla veri toplandıkça AI içgörüleri burada görünecek.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.25, delay }}
    >
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary" />
            AI İçgörüleri
          </CardTitle>
          <CardDescription>Yapay zeka destekli trendler, uyarılar ve öneriler</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Trends */}
          {trends.length > 0 && (
            <div className="space-y-3">
              <h3 className="text-sm font-semibold flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                Trendler
              </h3>
              <div className="space-y-2">
                {trends.map((trend, index) => (
                  <div
                    key={index}
                    className="flex items-start gap-3 p-3 rounded-lg border bg-muted/50"
                  >
                    {trend.isPositive ? (
                      <TrendingUp className="h-5 w-5 text-green-600 flex-shrink-0 mt-0.5" />
                    ) : (
                      <TrendingDown className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
                    )}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="text-sm font-medium">{trend.description}</span>
                        {trend.metric && (
                          <Badge variant={trend.isPositive ? 'default' : 'secondary'}>
                            {trend.metric}
                          </Badge>
                        )}
                      </div>
                      <span className="text-xs text-muted-foreground capitalize">{trend.type.replace('_', ' ')}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Alerts */}
          {alerts.length > 0 && (
            <div className="space-y-3">
              <h3 className="text-sm font-semibold flex items-center gap-2">
                <AlertTriangle className="h-4 w-4" />
                Uyarılar
              </h3>
              <div className="space-y-2">
                {alerts.map((alert, index) => (
                  <div
                    key={index}
                    className={`p-3 rounded-lg border flex items-start gap-3 ${
                      alert.severity === 'critical' || alert.severity === 'high'
                        ? 'border-red-500/50 bg-red-50 dark:bg-red-950/20'
                        : alert.severity === 'medium'
                        ? 'border-yellow-500/50 bg-yellow-50 dark:bg-yellow-950/20'
                        : 'border-gray-500/50 bg-gray-50 dark:bg-gray-950/20'
                    }`}
                  >
                    {getSeverityIcon(alert.severity)}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="text-sm font-semibold capitalize">{alert.severity}</span>
                        <Badge variant="outline" className="text-xs">
                          Uyarı
                        </Badge>
                      </div>
                      <p className="text-sm text-muted-foreground">{alert.message}</p>
                      {alert.suggestion && (
                        <p className="mt-1 text-xs italic text-muted-foreground">{alert.suggestion}</p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Recommendations */}
          {recommendations.length > 0 && (
            <div className="space-y-3">
              <h3 className="text-sm font-semibold flex items-center gap-2">
                <Lightbulb className="h-4 w-4" />
                Öneriler
              </h3>
              <div className="space-y-2">
                {recommendations
                  .sort((a, b) => b.priority - a.priority)
                  .map((rec, index) => (
                    <div
                      key={index}
                      className="p-4 rounded-lg border bg-card hover:bg-accent transition-colors"
                    >
                      <div className="flex items-start justify-between gap-3 mb-2">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <h4 className="font-medium text-sm">{rec.title}</h4>
                            <Badge variant="outline" className={getCategoryColor(rec.category)}>
                              {rec.category}
                            </Badge>
                            {rec.priority >= 4 && (
                              <Badge variant="default" className="text-xs bg-red-500 hover:bg-red-600">
                                Yüksek Öncelik
                              </Badge>
                            )}
                          </div>
                          <p className="text-sm text-muted-foreground">{rec.description}</p>
                        </div>
                      </div>
                      {rec.actionUrl && (
                        <Button variant="ghost" size="sm" className="mt-2" asChild>
                          <Link to={rec.actionUrl}>
                            Aksiyon Al
                            <ArrowRight className="ml-2 h-4 w-4" />
                          </Link>
                        </Button>
                      )}
                    </div>
                  ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}
